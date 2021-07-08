namespace Deathmatch.API.Matches
{
    public enum MatchStatus
    {
        /// <summary>
        /// Default value. Shouldn't be used.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// When a match has been initialized but <see cref="IMatch.StartAsync"/> has not yet been called.
        /// </summary>
        Initialized,

        /// <summary>
        /// When <see cref="IMatch.StartAsync"/> has been called, but before the task has ended.
        /// </summary>
        Starting,

        /// <summary>
        /// When <see cref="IMatch.StartAsync"/> has finished, but before <see cref="IMatch.EndAsync"/> has been called.
        /// </summary>
        InProgress,

        /// <summary>
        /// When <see cref="IMatch.EndAsync"/> has been called, but before the task has ended.
        /// </summary>
        Ending,

        /// <summary>
        /// When <see cref="IMatch.EndAsync"/> has finished.
        /// </summary>
        Ended,

        /// <summary>
        /// When an exception occurred either during <see cref="IMatch.StartAsync"/> or <see cref="IMatch.EndAsync"/>.
        /// </summary>
        Exception
    }
}
