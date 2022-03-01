using Deathmatch.API.Players;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;

namespace Deathmatch.API.Matches
{
    public static class MatchExecutorExtensions
    {
        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, Predicate<IGamePlayer> predicate)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => predicate(x));
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, ulong steamId)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x.SteamId.m_SteamID == steamId);
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, CSteamID steamId)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x.SteamId == steamId);
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, Player player)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x.Player == player);
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, UnturnedPlayer player)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x.SteamId == player.SteamId);
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, UnturnedUser user)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x.SteamId == user.SteamId);
        }

        public static IGamePlayer? GetParticipant(this IMatchExecutor matchExecutor, IGamePlayer player)
        {
            return matchExecutor.GetParticipants().FirstOrDefault(x => x == player);
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, ulong steamId)
        {
            return matchExecutor.GetParticipant(steamId) != null;
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, CSteamID steamId)
        {
            return matchExecutor.GetParticipant(steamId) != null;
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, Player player)
        {
            return matchExecutor.GetParticipant(player) != null;
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, UnturnedPlayer player)
        {
            return matchExecutor.GetParticipant(player) != null;
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, UnturnedUser user)
        {
            return matchExecutor.GetParticipant(user) != null;
        }

        public static bool IsParticipant(this IMatchExecutor matchExecutor, IGamePlayer player)
        {
            return matchExecutor.GetParticipant(player) != null;
        }
    }
}
