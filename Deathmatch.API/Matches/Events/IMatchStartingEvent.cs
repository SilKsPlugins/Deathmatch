using OpenMod.API.Eventing;

namespace Deathmatch.API.Matches.Events
{
    public interface IMatchStartingEvent : IMatchEvent, ICancellableEvent
    {
    }
}
