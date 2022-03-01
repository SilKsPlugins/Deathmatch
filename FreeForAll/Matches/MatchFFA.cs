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
using FreeForAll.Configuration;
using FreeForAll.Loadouts;
using FreeForAll.Players;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeForAll.Matches
{
    [Match("Free For All")]
    [MatchDescription("A game mode where the first to the kill threshold wins.")]
    [MatchAlias("FFA")]
    public class MatchFFA : MatchBase<FreeForAllConfig, FFALoadoutCategory>,
        IInstanceEventListener<UnturnedPlayerDeathEvent>,
        IInstanceEventListener<IGamePlayerSelectingRespawnEvent>,
        IInstanceEventListener<UnturnedPlayerSpawnedEvent>
    {
        private readonly SpawnDirectory _spawnDirectory;

        public MatchFFA(IServiceProvider serviceProvider,
            SpawnDirectory spawnDirectory) : base(serviceProvider)
        {
            _spawnDirectory = spawnDirectory;
        }

        public IReadOnlyCollection<PlayerSpawn> GetSpawns() => _spawnDirectory.Spawns;

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
                    Logger.LogError(ex, "Error occurred restoring player.");
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw exceptions.First();
            }

            IGamePlayer? winner = null;
            var maxKills = 0;

            Logger.LogDebug("FFA Match Kills:");

            foreach (var player in Players)
            {
                var playerKills = player.GetKills();

                Logger.LogDebug("{PlayerId} - {Kills}", player.SteamId, playerKills);

                if (playerKills > maxKills)
                {
                    winner = player;
                    maxKills = playerKills;
                }
                else if (playerKills == maxKills)
                {
                    winner = null;
                }
            }

            if (winner != null)
            {
                Logger.LogInformation(
                    "Player '{PlayerName}' ({PlayerSteamId}) has won the FFA match with {Kills} kills.",
                    winner.DisplayName, winner.SteamId, maxKills);

                await UserManager.BroadcastAsync(KnownActorTypes.Player,
                    StringLocalizer["announcements:match_end:player_won", new {Winner = winner.User}]);
            }
            else
            {
                Logger.LogInformation("The FFA match has ended in a tie with {Kills} kills.");

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

        /// <summary>
        /// Check win condition.
        /// </summary>
        public async UniTask HandleEventAsync(object? sender, UnturnedPlayerDeathEvent @event)
        {
            var victim = this.GetPlayer(@event.Player);
            var killer = this.GetPlayer(@event.Instigator);

            Logger.LogDebug("{VictimId} (IsNull: {VictimIsNull}) died to {KillerId} (IsNull: {KillerIsNull})", @event.Player.SteamId, @event.Instigator, victim == null, killer == null);

            if (victim == null || killer == null || killer == victim)
            {
                return;
            }

            var kills = killer.GetKills() + 1;

            killer.SetKills(kills);

            var threshold = Configuration.Instance.KillThreshold;

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
