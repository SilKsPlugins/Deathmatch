using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Deathmatch.Core.Grace;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Matches;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamDeathmatch.Players;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Matches
{
    [Match("Team Deathmatch")]
    [MatchDescription("A game mode where two teams fight to a kill threshold.")]
    [MatchAlias("TDM")]
    public class MatchTDM : MatchBase,
        IInstanceEventListener<UnturnedPlayerDamagingEvent>,
        IInstanceEventListener<UnturnedPlayerDeathEvent>,
        IInstanceEventListener<IGamePlayerSelectingRespawnEvent>
    {
        private readonly IPluginAccessor<TeamDeathmatchPlugin> _pluginAccessor;
        private readonly ILogger<MatchTDM> _logger;
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IGraceManager _graceManager;

        private CancellationTokenSource _tokenSource;

        private int _redDeaths;
        private int _blueDeaths;

        public MatchTDM(IPluginAccessor<TeamDeathmatchPlugin> pluginAccessor,
            ILogger<MatchTDM> logger,
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

            _tokenSource = new CancellationTokenSource();

            _redDeaths = 0;
            _blueDeaths = 0;
        }

        public IReadOnlyCollection<PlayerSpawn> GetSpawns(Team team)
        {
            return team switch
            {
                Team.Red => _pluginAccessor.Instance!.RedSpawns,
                Team.Blue => _pluginAccessor.Instance!.BlueSpawns,
                _ => throw new InvalidOperationException("Player has no team")
            };
        }

        public IReadOnlyCollection<PlayerSpawn> GetSpawns(IGamePlayer player) => GetSpawns(player.GetTeam());

        public async Task<ILoadout?> GetLoadout(IGamePlayer player)
        {
            const string category = "Team Deathmatch";

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

            if (GetPlayer(player.SteamId) != null) return;

            Players.Add(player);

            if (!IsRunning) return;

            int red = 0;
            int blue = 0;

            foreach (IGamePlayer otherPlayer in Players)
            {
                if (otherPlayer.GetTeam() == Team.Red) red++;
                else if (otherPlayer.GetTeam() == Team.Blue) blue++;
            }

            if (red < blue)
                player.SetTeam(Team.Red);
            else if (red > blue)
                player.SetTeam(Team.Blue);
            else
                player.SetTeam(Rng.NextDouble() < 0.5 ? Team.Red : Team.Blue);

            var spawn = GetSpawns(player).RandomElement();

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

            var redSpawns = GetSpawns(Team.Red).ToList().Shuffle();
            var blueSpawns = GetSpawns(Team.Blue).ToList().Shuffle();

            if (redSpawns.Count == 0 || blueSpawns.Count == 0)
            {
                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["errors:no_spawns"], Color.Red);

                return false;
            }

            IsRunning = true;
            HasRun = true;

            Team next = Rng.NextDouble() < 0.5 ? Team.Red : Team.Blue;

            foreach (var player in Players.Shuffle())
            {
                player.SetTeam(next);

                next = next == Team.Red ? Team.Blue : Team.Red;
            }

            int redSpawnIndex = 0;
            int blueSpawnIndex = 0;

            await UniTask.SwitchToMainThread();

            foreach (var player in Players)
            {
                await PreservationManager.PreservePlayer(player);

                PlayerSpawn spawn;

                if (player.GetTeam() == Team.Red)
                {
                    spawn = redSpawns[redSpawnIndex++];
                    redSpawnIndex %= redSpawns.Count;
                }
                else
                {
                    spawn = blueSpawns[blueSpawnIndex++];
                    blueSpawnIndex %= blueSpawns.Count;
                }

                await SpawnPlayer(player, spawn);
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

            Team winner = Team.None;

            if (_redDeaths > _blueDeaths)
            {
                winner = Team.Blue;

                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:blue_won"]);
            }
            else if (_redDeaths < _blueDeaths)
            {
                winner = Team.Red;

                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:red_won"]);
            }
            else
            {
                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:tie"]);
            }

            if (Players.Count >= Configuration.GetValue("Rewards:MinimumPlayers", 8))
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
                    if (winner == Team.None)
                        GiveRewards(player, tiedRewards);
                    else if (player.GetTeam() == winner)
                        GiveRewards(player, winnerRewards);
                    else
                        GiveRewards(player, loserRewards);

                    GiveRewards(player, allRewards);
                }
            }

            return true;
        }

        public UniTask HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
        {
            var victim = GetPlayer(@event.Player);
            var killer = GetPlayer(@event.Killer);

            if (victim != null && killer != null && victim != killer
                && victim.GetTeam() == killer.GetTeam()
                && !Configuration.GetValue("FriendlyFire", false))
            {
                @event.IsCancelled = true;
            }

            return UniTask.CompletedTask;
        }

        public async UniTask HandleEventAsync(object? sender, UnturnedPlayerDeathEvent @event)
        {
            var victim = GetPlayer(@event.Player);

            if (victim == null) return;

            var killer = GetPlayer(@event.Instigator);

            if (killer == null && @event.DeathCause != EDeathCause.BLEEDING) return;

            switch (victim.GetTeam())
            {
                case Team.Red:
                    _redDeaths++;
                    break;
                case Team.Blue:
                    _blueDeaths++;
                    break;
            }

            int threshold = Configuration.GetValue("DeathThreshold", 30);

            if (_redDeaths >= threshold || _blueDeaths >= threshold)
            {
                await MatchExecutor.EndMatch();
            }
        }

        /// <summary>
        /// Handles spawn point selection for the game player.
        /// </summary>
        public UniTask HandleEventAsync(object? sender, IGamePlayerSelectingRespawnEvent @event)
        {
            if (!IsRunning || @event.Player.CurrentMatch != this)
            {
                return UniTask.CompletedTask;
            }

            var spawn = GetSpawns(@event.Player).RandomElement();

            @event.WantsToSpawnAtHome = false;
            @event.Position = spawn.ToVector3().ToSystemVector();
            @event.Yaw = spawn.Yaw;

            return UniTask.CompletedTask;
        }
    }
}
