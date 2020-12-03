using System;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using System.Collections.Generic;
using OpenMod.Unturned.Users;

namespace Deathmatch.API.Players
{
    [Service]
    public interface IGamePlayerManager
    {
        IReadOnlyCollection<IGamePlayer> GetPlayers();
        IReadOnlyCollection<IGamePlayer> GetPlayers(Predicate<IGamePlayer> predicate);

        IGamePlayer GetPlayer(Predicate<IGamePlayer> predicate);
        IGamePlayer GetPlayer(UnturnedUser user);
        IGamePlayer GetPlayer(string searchString, UserSearchMode searchMode);
    }
}
