using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Deathmatch.Core.Grace;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Matches;
using Deathmatch.Core.Matches.Extensions;
using Deathmatch.Core.Spawns;
using FreeForAll.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Events;
using SilK.Unturned.Extras.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeForAll.Matches
{
    [Match("Free For All")]
    [MatchDescription("A game mode where the first to the kill threshold wins.")]
    [MatchAlias("FFA")]
    public class MatchFFA : MatchBase,
        IInstanceEventListener<UnturnedPlayerDeathEvent>,
        IInstanceEventListener<IGamePlayerSelectingRespawnEvent>
    {
        private readonly IPluginAccessor<FreeForAllPlugin> _pluginAccessor;
        private readonly ILogger<MatchFFA> _logger;
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IGraceManager _graceManager;

        public MatchFFA(
            IPluginAccessor<FreeForAllPlugin> pluginAccessor,
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

        public IReadOnlyCollection<PlayerSpawn> GetSpawns() => _pluginAccessor.Instance?.Spawns ??
                                                               throw new PluginNotLoadedException(
                                                                   typeof(FreeForAllPlugin));

        public PlayerSpawn GetFurthestSpawn()
        {
            var enemyPoints = Players.Where(x => !x.IsDead).Select(x => x.Transform.position).ToList();

            static float TotalMagnitude(Vector3 point, IEnumerable<Vector3> others)
            {
                return others.Sum(other => (other - point).sqrMagnitude);
            }

            var bestSpawn = GetSpawns()
                .Shuffle(Rng)
                .MaxBy(x => TotalMagnitude(x.ToVector3(), enemyPoints))
                .FirstOrDefault();

            return bestSpawn ?? throw new Exception("No spawns configured. Cannot spawn player.");
        }

        public async UniTask<ILoadout?> GetLoadout(IGamePlayer player)
        {
            const string category = "Free For All";

            var loadout = _loadoutSelector.GetLoadout(player, category);

            if (loadout != null && (loadout.Permission == null ||
                                    await _permissionChecker.CheckPermissionAsync(player.User, loadout.Permission) ==
                                    PermissionGrantResult.Grant))
            {
                return loadout;
            }

            return await _loadoutManager.GetRandomLoadout(category, player, _permissionChecker);
        }

        public async UniTask GiveLoadout(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

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

        public async UniTask SpawnPlayer(IGamePlayer player, PlayerSpawn spawn)
        {
            await UniTask.SwitchToMainThread();

            spawn.SpawnPlayer(player);

            player.Heal();

            await GiveLoadout(player);

            _graceManager.GrantGracePeriod(player, Configuration.GetValue<float>("GracePeriod", 2));
        }

        protected override async UniTask OnPlayerAdded(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            await PreservationManager.PreservePlayer(player);

            var spawn = GetFurthestSpawn();

            await SpawnPlayer(player, spawn);
        }

        protected override async UniTask OnPlayerRemoved(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            // Always restore, just in case
            await PreservationManager.RestorePlayer(player);
        }

        protected override async UniTask OnStartAsync()
        {
            var spawns = GetSpawns().ToList().Shuffle();

            if (spawns.Count == 0)
            {
                throw new UserFriendlyException(StringLocalizer["errors:no_spawns"]);
            }

            var spawnIndex = 0;

            await UniTask.SwitchToMainThread();

            foreach (var player in Players)
            {
                await PreservationManager.PreservePlayer(player);

                await SpawnPlayer(player, spawns[spawnIndex++]);
            }
            
            SetupDelayedEnd();
        }

        protected override async UniTask OnEndAsync()
        {
            await UniTask.SwitchToMainThread();

            var exceptions = new List<Exception>();

            foreach (var player in Players)
            {
                try
                {
                    await PreservationManager.RestorePlayer(player);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred restoring player.");
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw exceptions.First();
            }

            IGamePlayer? winner = null;
            var maxKills = 0;

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
        }

        private void SetupDelayedEnd()
        {
            var maxDuration = Configuration.GetValue("MaxDuration", 0f);

            if (maxDuration <= 0)
            {
                return;
            }

            async UniTask DelayedEnd(float delay)
            {
                await UniTask.Delay((int)(delay * 1000), cancellationToken: CancellationToken);

                await EndAsync();
            }

            UniTask.RunOnThreadPool(() => DelayedEnd(maxDuration)).Forget();
        }

        /// <summary>
        /// Check win condition.
        /// </summary>
        public async UniTask HandleEventAsync(object? sender, UnturnedPlayerDeathEvent @event)
        {
            var victim = this.GetPlayer(@event.Player);
            var killer = this.GetPlayer(@event.Instigator);

            if (victim == null || killer == null || killer == victim)
            {
                return;
            }

            var kills = killer.GetKills() + 1;

            killer.SetKills(kills);

            var threshold = Configuration.GetValue("KillThreshold", 30);

            if (kills >= threshold)
            {
                await EndAsync();
            }
        }

        /// <summary>
        /// Handles spawn point selection for the game player.
        /// </summary>
        public UniTask HandleEventAsync(object? sender, IGamePlayerSelectingRespawnEvent @event)
        {
            if (@event.Player.CurrentMatch != this || Status != MatchStatus.InProgress)
            {
                return UniTask.CompletedTask;
            }

            var spawn = GetFurthestSpawn();

            @event.WantsToSpawnAtHome = false;
            @event.Position = spawn.ToVector3().ToSystemVector();
            @event.Yaw = spawn.Yaw;

            return UniTask.CompletedTask;
        }
    }
}
