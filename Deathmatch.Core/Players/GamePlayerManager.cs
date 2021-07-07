using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Deathmatch.Core.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GamePlayerManager : IGamePlayerManager, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IRuntime _runtime;
        private readonly List<IGamePlayer> _players;

        public GamePlayerManager(IUserManager userManager,
            IEventBus eventBus,
            IRuntime runtime)
        {
            _eventBus = eventBus;
            _runtime = runtime;
            _players = new List<IGamePlayer>();

            AsyncHelper.RunSync(async () =>
            {
                var existingPlayers = await userManager.GetUsersAsync(KnownActorTypes.Player);

                foreach (var user in existingPlayers.OfType<UnturnedUser>())
                {
                    var player = new GamePlayer(user);

                    _players.Add(player);

                    await _eventBus.EmitAsync(_runtime, this, new GamePlayerConnectedEvent(player));
                }
            });

            PlayerLife.OnSelectingRespawnPoint += OnSelectingRespawnPoint;
        }

        public void Dispose()
        {
            PlayerLife.OnSelectingRespawnPoint -= OnSelectingRespawnPoint;
        }

        internal async Task AddUser(UnturnedUser user)
        {
            if (_players.All(x => x.SteamId != user.SteamId))
            {
                var player = new GamePlayer(user);

                _players.Add(player);

                await _eventBus.EmitAsync(_runtime, this, new GamePlayerConnectedEvent(player));
            }
        }

        internal async Task RemoveUser(UnturnedUser user)
        {
            var player = _players.FirstOrDefault(x => x.SteamId != user.SteamId);

            if (player != null)
            {
                await _eventBus.EmitAsync(_runtime, this, new GamePlayerDisconnectedEvent(player));
                _players.RemoveAll(x => x.SteamId == user.SteamId);
            }
        }

        public IReadOnlyCollection<IGamePlayer> GetPlayers()
        {
            return _players.AsReadOnly();
        }

        public IReadOnlyCollection<IGamePlayer> GetPlayers(Predicate<IGamePlayer> predicate)
        {
            return _players.Where(predicate.Invoke).ToList().AsReadOnly();
        }

        public IGamePlayer? GetPlayer(Predicate<IGamePlayer> predicate)
        {
            return _players.FirstOrDefault(predicate.Invoke);
        }

        public IGamePlayer? GetPlayer(string searchString, UserSearchMode searchMode)
        {
            IGamePlayer? player = null;

            if (searchMode == UserSearchMode.FindById || searchMode == UserSearchMode.FindByNameOrId)
            {
                if (ulong.TryParse(searchString, out var id))
                {
                    player = this.GetPlayer(id);
                }
            }

            if (searchMode == UserSearchMode.FindByName || searchMode == UserSearchMode.FindByNameOrId)
            {
                player ??= _players.FindBestMatch(x => x.DisplayName, searchString);
            }

            return player;
        }

        private void OnSelectingRespawnPoint(PlayerLife sender, bool wantsToSpawnAtHome, ref Vector3 position, ref float yaw)
        {
            var player = this.GetPlayer(sender.player);

            var @event =
                new GamePlayerSelectingRespawnEvent(player, wantsToSpawnAtHome, position.ToSystemVector(), yaw);

            AsyncHelper.RunSync(() => _eventBus.EmitAsync(_runtime, this, @event));

            position = @event.Position.ToUnityVector();
            yaw = @event.Yaw;
        }
    }
}
