using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.Core.Grace;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Matches;
using Deathmatch.Core.Spawns;
using FreeForAll.Players;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Color = System.Drawing.Color;

namespace FreeForAll.Matches
{
    [Match("Free For All")]
    [MatchDescription("A game mode where the first to the kill threshold wins.")]
    [MatchAlias("FFA")]
    public class MatchFFA : MatchBase,
        IMatchEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IPluginAccessor<FreeForAllPlugin> _pluginAccessor;
        private readonly ILogger<MatchFFA> _logger;
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IGraceManager _graceManager;

        private CancellationTokenSource _tokenSource;

        public MatchFFA(IPluginAccessor<FreeForAllPlugin> pluginAccessor,
            ILogger<MatchFFA> logger,
            ILoadoutManager loadoutManager,
            ILoadoutSelector loadoutSelector,
            IPermissionChecker permissionChecker,
            IGraceManager graceManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _pluginAccessor = pluginAccessor;
            _logger = logger;
            _loadoutManager = loadoutManager;
            _loadoutSelector = loadoutSelector;
            _permissionChecker = permissionChecker;
            _graceManager = graceManager;
        }

        public IReadOnlyCollection<PlayerSpawn> GetSpawns() => _pluginAccessor.Instance.Spawns;

        public PlayerSpawn GetFurthestSpawn()
        {
            var enemyPoints = GetPlayers().Where(x => !x.IsDead).Select(x => x.Transform.position);

            float TotalMagnitude(Vector3 point, IEnumerable<Vector3> others)
            {
                float total = 0;

                foreach (var other in others)
                {
                    total += (other - point).sqrMagnitude;
                }

                return total;
            }

            var spawns = GetSpawns().ToList().Shuffle();

            PlayerSpawn best = null;
            float bestDist = 0;

            foreach (var spawn in spawns)
            {
                var dist = TotalMagnitude(spawn.ToVector3(), enemyPoints);

                if (dist < bestDist) continue;

                best = spawn;
                bestDist = dist;
            }

            return best;
        }

        public async Task<ILoadout> GetLoadout(IGamePlayer player)
        {
            const string category = "Free For All";

            var loadout = _loadoutSelector.GetLoadout(player, category);

            if (loadout != null && await _permissionChecker.CheckPermissionAsync(player.User, loadout.Permission) ==
                PermissionGrantResult.Grant)
            {
                return loadout;
            }

            return await _loadoutManager.GetRandomLoadout(category, player, _permissionChecker);
        }

        public async Task GiveLoadout(IGamePlayer player)
        {
            var loadout = await GetLoadout(player);

            if (loadout == null)
            {
                player.ClearInventory();
                player.ClearClothing();
            }
            else
            {
                loadout.GiveToPlayer(player);
            }
        }

        public async Task SpawnPlayer(IGamePlayer player, PlayerSpawn spawn)
        {
            spawn.SpawnPlayer(player);

            player.Heal();

            await GiveLoadout(player);

            _graceManager.GrantGracePeriod(player, Configuration.GetValue<float>("GracePeriod", 2));
        }

        public override async UniTask AddPlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            if (GetPlayer(player) != null) return;

            Players.Add(player);

            if (!IsRunning) return;

            var spawn = GetFurthestSpawn();

            await PreservationManager.PreservePlayer(player);

            await SpawnPlayer(player, spawn);
        }

        public override async UniTask AddPlayers(IEnumerable<IGamePlayer> players)
        {
            await UniTask.SwitchToMainThread();

            foreach (var player in players)
            {
                await AddPlayer(player);
            }
        }

        public override async UniTask RemovePlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            if (GetPlayer(player) == null) return;

            Players.Remove(player);

            // Always restore, just in case
            await PreservationManager.RestorePlayer(player);
        }

        public override async UniTask<bool> StartMatch()
        {
            if (IsRunning) return false;

            var spawns = GetSpawns().ToList().Shuffle();

            if (spawns.Count == 0)
            {
                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["errors:no_spawns"], Color.Red);

                return false;
            }

            IsRunning = true;
            HasRun = true;

            OnReviving += Events_OnReviving;

            int spawnIndex = 0;

            await UniTask.SwitchToMainThread();

            foreach (var player in Players)
            {
                await PreservationManager.PreservePlayer(player);

                await SpawnPlayer(player, spawns[spawnIndex++]);
            }

            _tokenSource = new CancellationTokenSource();

            int maxDuration = Configuration.GetSection("MaxDuration").Get<int>();

            if (maxDuration > 0)
            {
                async UniTask DelayedEnd(int delay)
                {
                    await UniTask.Delay(delay * 1000, cancellationToken: _tokenSource.Token);

                    if (IsRunning)
                    {
                        await EndMatch();
                    }
                }

                DelayedEnd(maxDuration).Forget();
            }

            return true;
        }

        public override async UniTask<bool> EndMatch()
        {
            if (!IsRunning) return false;

            IsRunning = false;

            OnReviving -= Events_OnReviving;

            _tokenSource?.Cancel();

            await UniTask.SwitchToMainThread();

            foreach (var player in Players)
            {
                try
                {
                    await PreservationManager.RestorePlayer(player);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred restoring player.");
                    throw;
                }
            }

            IGamePlayer winner = null;
            int maxKills = 0;

            foreach (var player in Players)
            {
                if (player.GetKills() > maxKills)
                {
                    winner = player;
                    maxKills = player.GetKills();
                }
                else if (player.GetKills() == maxKills)
                {
                    winner = null;
                }
            }

            if (winner != null)
            {
                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:player_won", new { Winner = winner.User }]);
            }
            else
            {
                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:tie"]);
            }

            if (Players.Count >= Configuration.GetValue("Rewards:MinimumPlayers", 5))
            {
                var winnerRewards =
                    Configuration.GetSection("Rewards:Winners").Get<List<ChanceItem>>() ?? new List<ChanceItem>();
                var loserRewards =
                    Configuration.GetSection("Rewards:Losers").Get<List<ChanceItem>>() ?? new List<ChanceItem>();
                var tiedRewards =
                    Configuration.GetSection("Rewards:Tied").Get<List<ChanceItem>>() ?? new List<ChanceItem>();
                var allRewards =
                    Configuration.GetSection("Rewards:All").Get<List<ChanceItem>>() ?? new List<ChanceItem>();

                void GiveRewards(IGamePlayer player, List<ChanceItem> items)
                {
                    foreach (var item in items)
                    {
                        item.GiveToPlayer(player);
                    }
                }

                foreach (var player in Players)
                {
                    if (winner == null)
                        GiveRewards(player, tiedRewards);
                    else if (player == winner)
                        GiveRewards(player, winnerRewards);
                    else
                        GiveRewards(player, loserRewards);

                    GiveRewards(player, allRewards);
                }
            }

            return true;
        }

        private void Events_OnReviving(Player nativePlayer, ref bool cancel)
        {
            if (!IsRunning) return;

            var player = GetPlayer(nativePlayer);

            cancel = true;

            var spawn = GetFurthestSpawn();

            AsyncHelper.RunSync(() => SpawnPlayer(player, spawn));
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
        {
            var victim = GetPlayer(@event.Player);

            if (victim == null) return;

            var killer = GetPlayer(@event.Instigator);

            if (killer == null) return;

            if (killer == victim) return;

            var kills = killer.GetKills() + 1;

            killer.SetKills(kills);

            var threshold = Configuration.GetValue("KillThreshold", 30);

            if (kills >= threshold)
                await MatchExecutor.EndMatch();
        }

        private delegate void Reviving(Player player, ref bool cancel);
        private static event Reviving OnReviving;

        // ReSharper disable InconsistentNaming, UnusedType.Local, UnusedMember.Local
        [HarmonyPatch]
        private static class Patches
        {
            [HarmonyPatch(typeof(PlayerLife), "askRespawn")]
            [HarmonyPrefix]
            private static bool PreAskRespawn(PlayerLife __instance)
            {
                bool cancel = false;

                OnReviving?.Invoke(__instance.player, ref cancel);

                return !cancel;
            }
        }
        // ReSharper restore InconsistentNaming, UnusedType.Local, UnusedMember.Local
    }
}
