using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Deathmatch.API.Players
{
    [Service]
    public interface IGamePlayerManager
    {
        IReadOnlyCollection<IGamePlayer> GetPlayers();

        IReadOnlyCollection<IGamePlayer> GetPlayers(Predicate<IGamePlayer> predicate);

        IGamePlayer GetPlayer(Predicate<IGamePlayer> predicate);
        
        IGamePlayer GetPlayer(string searchString, UserSearchMode searchMode);
    }
}
