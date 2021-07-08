using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Matches.Extensions
{
    public static class MatchExtensions
    {
        /// <summary>
        /// Add players to this match.
        /// </summary>
        /// <param name="match">The match players are being added to.</param>
        /// <param name="players">The players to add.</param>
        public static async UniTask AddPlayers(this IMatch match, IEnumerable<IGamePlayer> players)
        {
            await match.AddPlayers(players.ToArray());
        }

        /// <summary>
        /// Remove players from this match.
        /// </summary>
        /// <param name="match">The match players are being removed from.</param>
        /// <param name="players">The players to remove.</param>
        public static async UniTask RemovePlayers(this IMatch match, IEnumerable<IGamePlayer> players)
        {
            await match.RemovePlayers(players.ToArray());
        }


        /// <summary>
        /// Adds a player to this match.
        /// </summary>
        /// <param name="match">The match a player is being added to.</param>
        /// <param name="player">The player to add.</param>
        public static async UniTask AddPlayer(this IMatch match, IGamePlayer player)
        {
            await match.AddPlayers(player);
        }

        /// <summary>
        /// Remove a player to this match.
        /// </summary>
        /// <param name="match">The match a player is being removed from.</param>
        /// <param name="player">The player to remove.</param>
        public static async UniTask RemovePlayer(this IMatch match, IGamePlayer player)
        {
            await match.RemovePlayers(player);
        }

        /// <summary>
        /// Get a player based on the given predicate.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="predicate">The predicate checking for a match.</param>
        /// <returns>If found, the first player matching the predicate; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, Predicate<IGamePlayer> predicate)
        {
            return match.Players.FirstOrDefault(x => predicate(x));
        }

        /// <summary>
        /// Get players based on the given predicate.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="predicate">The predicate checking for a match.</param>
        /// <returns>The players matching the predicate.</returns>
        public static IEnumerable<IGamePlayer> GetPlayers(this IMatch match, Predicate<IGamePlayer> predicate)
        {
            return match.Players.Where(x => predicate(x));
        }

        /// <summary>
        /// Get a player in the match from their Steam ID.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="steamId">The steam ID of the target player.</param>
        /// <returns>If found, the player with the Steam ID; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, ulong steamId)
        {
            return match.GetPlayer(gamePlayer => gamePlayer.SteamId.m_SteamID == steamId);
        }

        /// <summary>
        /// Get a player in the match from their Steam ID.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="steamId">The steam ID of the target player.</param>
        /// <returns>If found, the player with the Steam ID; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, CSteamID steamId)
        {
            return match.GetPlayer(gamePlayer => gamePlayer.SteamId == steamId);
        }

        /// <summary>
        /// Get a player in the match from their native player instance.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="player">The native player.</param>
        /// <returns>If found, the player matching the native player instance; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, Player player)
        {
            return match.GetPlayer(gamePlayer => gamePlayer.SteamId == player.channel.owner.playerID.steamID);
        }

        /// <summary>
        /// Get a player in the match from their player instance.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="player">The player.</param>
        /// <returns>If found, the player matching the player instance; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, UnturnedPlayer player)
        {
            return match.GetPlayer(p => p.SteamId == player.SteamId);
        }

        /// <summary>
        /// Get a player in the match from their user instance.
        /// </summary>
        /// <param name="match">The match players are being searched in.</param>
        /// <param name="user">The user.</param>
        /// <returns>If found, the player matching the user instance; otherwise <b>null</b>.</returns>
        public static IGamePlayer? GetPlayer(this IMatch match, UnturnedUser user)
        {
            return match.GetPlayer(p => p.SteamId == user.SteamId);
        }
    }
}
