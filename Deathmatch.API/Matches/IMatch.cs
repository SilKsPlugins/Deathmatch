using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using System.Collections.Generic;

namespace Deathmatch.API.Matches
{
    public interface IMatch
    {
        IMatchRegistration Registration { get; set; }

        bool IsRunning { get; }
        bool HasRun { get; }

        /// <summary>
        /// Starts this match instance.
        /// <para>Should only be called from an <c>IMatchExecutor</c> implementation.</para>
        /// </summary>
        /// <returns><c>true</c> if match was started successfully, <c>false</c> otherwise.</returns>
        UniTask<bool> StartMatch();

        /// <summary>
        /// Ends this match instance.
        /// <para>Should only be called from an <c>IMatchExecutor</c> implementation.</para>
        /// </summary>
        /// <returns><c>true</c> if match was ended successfully, <c>false</c> otherwise.</returns>
        UniTask<bool> EndMatch();

        IReadOnlyCollection<IGamePlayer> GetPlayers();

        UniTask AddPlayer(IGamePlayer player);
        UniTask AddPlayers(IEnumerable<IGamePlayer> player);

        UniTask RemovePlayer(IGamePlayer user);
    }
}
