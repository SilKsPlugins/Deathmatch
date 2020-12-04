using Deathmatch.API.Players;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using StringHelper = Deathmatch.Core.Helpers.StringHelper;

namespace Deathmatch.Core.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GamePlayerManager : IGamePlayerManager
    {
        private readonly List<IGamePlayer> _players;

        public GamePlayerManager(IUserManager userManager)
        {
            _players = new List<IGamePlayer>();

            AsyncHelper.RunSync(async () =>
            {
                var existingPlayers = await userManager.GetUsersAsync(KnownActorTypes.Player);

                foreach (var player in existingPlayers.OfType<UnturnedUser>())
                {
                    OnUserConnected(player);
                }
            });
        }

        internal void OnUserConnected(UnturnedUser user)
        {
            if (user == null) return;

            if (_players.Any(x => x.SteamId == user.SteamId))
                return;

            var player = new GamePlayer(user);

            _players.Add(player);
        }

        internal void OnUserDisconnected(UnturnedUser user)
        {
            if (user == null) return;

            _players.RemoveAll(x => x.SteamId == user.SteamId);
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
