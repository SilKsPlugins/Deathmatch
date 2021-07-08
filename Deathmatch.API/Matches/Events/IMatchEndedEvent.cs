namespace Deathmatch.API.Matches.Events
{
    /// <summary>
    /// This event is emitted after a match has ended.
    /// <see cref="IMatch.Players"/> will empty and so will each player's match data.
    /// To access players and their match data, use <see cref="IMatchEndingEvent"/>.
    /// </summary>
    public interface IMatchEndedEvent : IMatchEvent
    {
    }
}
