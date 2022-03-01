using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches.Registrations;
using Deathmatch.API.Players;
using OpenMod.API.Ioc;
using System.Collections.Generic;

namespace Deathmatch.API.Matches
{
    [Service]
    public interface IMatchExecutor
    {
        /// <summary>
        /// The current running match.
        /// </summary>
        IMatch? CurrentMatch { get; }

        /// <summary>
        /// Gets all players in the current match pool.
        /// </summary>
        /// <returns>All players in the current match pool.</returns>
        IReadOnlyCollection<IGamePlayer> GetParticipants();

        /// <summary>
        /// Adds the player to the match pool and also the current match if one is running.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <returns><b>true</b> if the player was added to the match pool, <b>false</b> otherwise.</returns>
        UniTask<bool> AddParticipant(IGamePlayer player);


        UniTask RemoveParticipant(IGamePlayer user);

        /// <summary>
        /// Attempts to start a match.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns><b>true</b> when a match is started successfully. <b>false</b> if a match is already running.</returns>
        /// <exception cref="OpenMod.API.Commands.UserFriendlyException">When a match could not be started due to user error.</exception>
        UniTask<bool> StartMatch(IMatchRegistration? registration = null);
    }
}
