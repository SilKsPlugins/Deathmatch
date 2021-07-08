namespace Deathmatch.API.Matches.Events
{
    /// <summary>
    /// This event is emitted before a match is cleaned up and considered fully ended.
    /// This event is not cancellable, it is simply called before ending the match.
    /// </summary>
    public interface IMatchEndingEvent : IMatchEvent
    {
    }
}
