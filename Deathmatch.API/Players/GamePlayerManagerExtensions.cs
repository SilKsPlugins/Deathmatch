using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;

namespace Deathmatch.API.Players
{
    public static class GamePlayerManagerExtensions
    {
        public static IGamePlayer GetPlayer(this IGamePlayerManager manager, CSteamID steamId)
        {
            return manager.GetPlayer(x => x.SteamId == steamId);
        }

        public static IGamePlayer GetPlayer(this IGamePlayerManager manager, ulong steamId)
        {
            return manager.GetPlayer(new CSteamID(steamId));
        }

        public static IGamePlayer GetPlayer(this IGamePlayerManager manager, Player player)
        {
            return manager.GetPlayer(player.channel.owner.playerID.steamID);
        }

        public static IGamePlayer GetPlayer(this IGamePlayerManager manager, UnturnedPlayer player)
        {
            return manager.GetPlayer(player.SteamId);
        }

        public static IGamePlayer GetPlayer(this IGamePlayerManager manager, UnturnedUser user)
        {
            return manager.GetPlayer(user.SteamId);
        }
    }
}
