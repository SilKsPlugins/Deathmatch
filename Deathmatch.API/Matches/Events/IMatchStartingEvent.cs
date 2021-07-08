using OpenMod.API.Eventing;

namespace Deathmatch.API.Matches.Events
{
    /// <summary>
    /// This event is emitted before a match has started.
    /// The match can be cancelled by setting <see cref="IMatchStartingEvent.IsCancelled"/> to <c>true</c>.
    /// </summary>
    public interface IMatchStartingEvent : IMatchEvent, ICancellableEvent
    {
    }
}
