using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches.Registrations;
using Deathmatch.API.Players;
using System;
using System.Collections.Generic;

namespace Deathmatch.API.Matches
{
    public interface IMatch
    {
        /// <summary>
        /// This matches registration.
        /// </summary>
        IMatchRegistration Registration { get; }

        /// <summary>
        /// The lifetime scope for this match.
        /// </summary>
        ILifetimeScope LifetimeScope { get; }

        /// <summary>
        /// The current status of this match.
        /// </summary>
        MatchStatus Status { get; }

        /// <summary>
        /// The players in this match.
        /// </summary>
        IReadOnlyCollection<IGamePlayer> Players { get; }

        /// <summary>
        /// Starts this match instance.
        /// <para>Should only be called from an <see cref="IMatchExecutor"/> implementation.</para>
        /// <param name="players">The initial set of players for the match.</param>
        /// </summary>
        UniTask StartAsync(IEnumerable<IGamePlayer> players);

        /// <summary>
        /// Ends this match instance.
        /// <para>This is safe to call outside of <see cref="IMatchExecutor"/> implementations unlike <see cref="StartAsync"/>.</para>
        /// </summary>
        UniTask EndAsync();

        /// <summary>
        /// Add players to this match.
        /// </summary>
        /// <param name="players">The players to add.</param>
        UniTask AddPlayers(params IGamePlayer[] players);

        /// <summary>
        /// Remove players from this match.
        /// </summary>
        /// <param name="players">The players to remove.</param>
        UniTask RemovePlayers(params IGamePlayer[] players);
    }
}
