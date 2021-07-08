using OpenMod.API.Eventing;

namespace Deathmatch.API.Matches.Events
{
    /// <summary>
    /// An event related to a match.
    /// </summary>
    public interface IMatchEvent : IEvent
    {
        IMatch Match { get; }
    }
}
