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
using Microsoft.Extensions.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IGraceManager _graceManager;

        private int _redDeaths;
        private int _blueDeaths;

        private int _redInitialSpawnIndex;
        private int _blueInitialSpawnIndex;

        private readonly List<PlayerSpawn> _redInitialSpawns;
        private readonly List<PlayerSpawn> _blueInitialSpawns;

        public MatchTDM(IPluginAccessor<TeamDeathmatchPlugin> pluginAccessor,
            ILoadoutManager loadoutManager,
            ILoadoutSelector loadoutSelector,
            IPermissionChecker permissionChecker,
            IGraceManager graceManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _pluginAccessor = pluginAccessor;
            _loadoutManager = loadoutManager;
            _loadoutSelector = loadoutSelector;
            _permissionChecker = permissionChecker;
            _graceManager = graceManager;

            _redDeaths = 0;
            _blueDeaths = 0;

            _redInitialSpawnIndex = 0;
            _blueInitialSpawnIndex = 0;

            _redInitialSpawns = new List<PlayerSpawn>();
            _blueInitialSpawns = new List<PlayerSpawn>();
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

            PlayerSpawn spawn;

            var red = 0;
            var blue = 0;

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

            if (Status == MatchStatus.Starting)
            {
                if (player.GetTeam() == Team.Red)
                {
                    _redInitialSpawnIndex = (_redInitialSpawnIndex + 1) % _redInitialSpawns.Count;
                    spawn = _redInitialSpawns[_redInitialSpawnIndex];
                }
                else
                {
                    _blueInitialSpawnIndex = (_blueInitialSpawnIndex + 1) % _blueInitialSpawns.Count;
                    spawn = _blueInitialSpawns[_blueInitialSpawnIndex];
                }
            }
            else
            {
                spawn = GetSpawns(player).RandomElement();
            }

            await SpawnPlayer(player, spawn);
        }

        protected override async UniTask OnPlayerRemoved(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            // Always restore, just in case
            await PreservationManager.RestorePlayer(player);
        }

        protected override UniTask OnStartAsync()
        {
            var redSpawns = GetSpawns(Team.Red).ToList().Shuffle();
            var blueSpawns = GetSpawns(Team.Blue).ToList().Shuffle();

            if (redSpawns.Count == 0 || blueSpawns.Count == 0)
            {
                return UniTask.FromException(new UserFriendlyException(StringLocalizer["errors:no_spawns"]));
            }

            _redInitialSpawns.AddRange(redSpawns);
            _blueInitialSpawns.AddRange(blueSpawns);

            SetupDelayedEnd();

            return UniTask.CompletedTask;
        }

        protected override async UniTask OnEndAsync()
        {
            await UniTask.SwitchToMainThread();

            var winner = Team.None;

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
                    {
                        GiveRewards(player, tiedRewards);
                    }
                    else if (player.GetTeam() == winner)
                    {
                        GiveRewards(player, winnerRewards);
                    }
                    else
                    {
                        GiveRewards(player, loserRewards);
                    }

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

        public UniTask HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
        {
            var victim = this.GetPlayer(@event.Player);
            var killer = this.GetPlayer(@event.Killer);

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
            var victim = this.GetPlayer(@event.Player);

            if (victim == null)
            {
                return;
            }

            var killer = this.GetPlayer(@event.Instigator);

            if (killer == null && @event.DeathCause != EDeathCause.BLEEDING)
            {
                return;
            }

            if (victim == killer)
            {
                return;
            }

            switch (victim.GetTeam())
            {
                case Team.Red:
                    _redDeaths++;
                    break;
                case Team.Blue:
                    _blueDeaths++;
                    break;
            }

            var threshold = Configuration.GetValue("DeathThreshold", 30);

            if (_redDeaths >= threshold || _blueDeaths >= threshold)
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

            var spawn = GetSpawns(@event.Player).RandomElement();

            @event.WantsToSpawnAtHome = false;
            @event.Position = spawn.ToVector3().ToSystemVector();
            @event.Yaw = spawn.Yaw;

            return UniTask.CompletedTask;
        }
    }
}
