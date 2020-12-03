using Deathmatch.API.Matches.Events;
using OpenMod.API.Eventing;

namespace Deathmatch.API.Players.Events
{
    public interface IGamePlayerJoiningMatchEvent : IGamePlayerEvent, IMatchEvent, ICancellableEvent
    {
    }
}
