using Deathmatch.API.Players;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StringHelper = Deathmatch.Core.Helpers.StringHelper;

namespace Deathmatch.Core.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GamePlayerManager : IGamePlayerManager
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

            eventBus.Subscribe(runtime, (EventCallback<UnturnedUserConnectedEvent>)OnUserConnected);
            eventBus.Subscribe(runtime, (EventCallback<UnturnedUserDisconnectedEvent>)OnUserDisconnected);

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
        }

        private async Task OnUserConnected(IServiceProvider serviceProvider, object sender, UnturnedUserConnectedEvent @event)
        {
            if (_players.All(x => x.SteamId != @event.User.SteamId))
            {
                var player = new GamePlayer(@event.User);

                _players.Add(player);

                await _eventBus.EmitAsync(_runtime, this, new GamePlayerConnectedEvent(player));
            }

            return Task.CompletedTask;
        }

        private Task OnUserDisconnected(IServiceProvider serviceProvider, object sender, UnturnedUserDisconnectedEvent @event)
        {
            var player = GetPlayer(@event.User);

            if (player != null)
            {
                _eventBus.EmitAsync(_runtime, this, new GamePlayerDisconnectedEvent(player));
                _players.RemoveAll(x => x.SteamId == @event.User.SteamId);
            }

            return Task.CompletedTask;
        }

        public IReadOnlyCollection<IGamePlayer> GetPlayers()
        {
            return _players.AsReadOnly();
        }

        public IReadOnlyCollection<IGamePlayer> GetPlayers(Predicate<IGamePlayer> predicate)
        {
            return _players.Where(predicate.Invoke).ToList().AsReadOnly();
        }

        public IGamePlayer GetPlayer(CSteamID steamId) => GetPlayer(x => x.SteamId == steamId);

        public IGamePlayer GetPlayer(Player player) => GetPlayer(player.channel.owner.playerID.steamID);

        public IGamePlayer GetPlayer(UnturnedPlayer player) => player == null ? null : GetPlayer(player.SteamId);

        public IGamePlayer GetPlayer(UnturnedUser user) => user == null ? null : GetPlayer(x => x.User.Equals(user));

        public IGamePlayer GetPlayer(Predicate<IGamePlayer> predicate)
        {
            return _players.FirstOrDefault(predicate.Invoke);
        }

        public IGamePlayer GetPlayer(string searchString, UserSearchMode searchMode)
        {
            IGamePlayer player = null;

            if (searchMode == UserSearchMode.FindById || searchMode == UserSearchMode.FindByNameOrId)
            {
                if (ulong.TryParse(searchString, out var id))
                {
                    player = GetPlayer(x => x.SteamId.m_SteamID == id);

                    if (player != null)
                        return player;
                }

                if (searchMode == UserSearchMode.FindById)
                {
                    return null;
                }
            }

            searchString = searchString.ToLower();

            int minDist = 0;

            foreach (var gamePlayer in _players)
            {
                string name = gamePlayer.DisplayName.ToLower();

                if (!name.Contains(searchString)) continue;

                int dist = StringHelper.Distance(name, searchString);

                if (player == null || minDist < dist)
                {
                    player = gamePlayer;
                    minDist = dist;
                }
            }

            return player;
        }
    }
}
