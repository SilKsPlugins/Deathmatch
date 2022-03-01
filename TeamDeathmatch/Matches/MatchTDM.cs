using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Matches;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamDeathmatch.Configuration;
using TeamDeathmatch.Loadouts;
using TeamDeathmatch.Players;
using TeamDeathmatch.Spawns;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Matches
{
    [Match("Team Deathmatch")]
    [MatchDescription("A game mode where two teams fight to a kill threshold.")]
    [MatchAlias("TDM")]
    public class MatchTDM : MatchBase<TeamDeathmatchConfiguration, TDMLoadoutCategory>,
        IInstanceEventListener<UnturnedPlayerDamagingEvent>,
        IInstanceEventListener<UnturnedPlayerDeathEvent>,
        IInstanceEventListener<UnturnedPlayerSpawnedEvent>,
        IInstanceEventListener<IGamePlayerSelectingRespawnEvent>
    {
        private readonly BlueSpawnDirectory _blueSpawnDirectory;
        private readonly RedSpawnDirectory _redSpawnDirectory;

        private int _redDeaths;
        private int _blueDeaths;

        private int _redInitialSpawnIndex;
        private int _blueInitialSpawnIndex;

        private readonly List<PlayerSpawn> _redInitialSpawns;
        private readonly List<PlayerSpawn> _blueInitialSpawns;

        public MatchTDM(IServiceProvider serviceProvider,
            BlueSpawnDirectory blueSpawnDirectory,
            RedSpawnDirectory redSpawnDirectory) : base(serviceProvider)
        {
            _blueSpawnDirectory = blueSpawnDirectory;
            _redSpawnDirectory = redSpawnDirectory;

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
                Team.Red => _redSpawnDirectory.Spawns,
                Team.Blue => _blueSpawnDirectory.Spawns,
                _ => throw new InvalidOperationException("Player has no team")
            };
        }

        public IReadOnlyCollection<PlayerSpawn> GetSpawns(IGamePlayer player) => GetSpawns(player.GetTeam());

        public async Task<ILoadout?> GetLoadout(IGamePlayer player)
        {
            var loadout = LoadoutSelector.GetSelectedLoadout(player, LoadoutCategory);

            if (loadout != null && await loadout.IsPermitted(player.User))
            {
                return loadout;
            }

            return await LoadoutCategory.GetRandomLoadout(player);
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
                await loadout.GiveToPlayer(player);
            }
        }

        public async UniTask GrantGracePeriod(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            GraceManager.GrantGracePeriod(player, Configuration.Instance.GracePeriod);
        }

        public async UniTask SpawnPlayer(IGamePlayer player, PlayerSpawn? spawn = null)
        {
            await UniTask.SwitchToMainThread();

            try
            {
                await GrantGracePeriod(player);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when granting player grace period");
            }

            spawn?.SpawnPlayer(player);

            try
            {
                player.Heal();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when healing player");
            }

            try
            {
                await GiveLoadout(player);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred when giving player loadout");
            }
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

            if (Players.Count >= Configuration.Instance.Rewards.MinimumPlayers)
            {
                var winnerRewards = Configuration.Instance.Rewards.Winners;
                var loserRewards = Configuration.Instance.Rewards.Losers;
                var tiedRewards = Configuration.Instance.Rewards.Tied;
                var allRewards = Configuration.Instance.Rewards.All;

                void GiveRewards(IGamePlayer player, IEnumerable<ChanceItem> items)
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
            var maxDuration = Configuration.Instance.MaxDuration;

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
                && !Configuration.Instance.FriendlyFire)
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

            if (killer == null || victim == killer)
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

            var threshold = Configuration.Instance.KillThreshold;

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

        public async UniTask HandleEventAsync(object? sender, UnturnedPlayerSpawnedEvent @event)
        {
            var player = this.GetPlayer(@event.Player);

            if (player == null || Status != MatchStatus.InProgress)
            {
                return;
            }

            await SpawnPlayer(player);
        }
    }
}
