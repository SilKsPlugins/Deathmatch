using OpenMod.API.Eventing;

namespace Deathmatch.API.Players.Events
{
    public interface IGamePlayerEvent : IEvent
    {
        IGamePlayer Player { get; }
    }
}
