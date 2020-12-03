using OpenMod.API.Eventing;

namespace Deathmatch.API.Matches.Events
{
    public interface IMatchEvent : IEvent
    {
        IMatch Match { get; }
    }
}
