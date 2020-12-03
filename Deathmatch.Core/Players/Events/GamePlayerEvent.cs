using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using OpenMod.Core.Eventing;

namespace Deathmatch.Core.Players.Events
{
    public abstract class GamePlayerEvent : Event, IGamePlayerEvent
    {
        public IGamePlayer Player { get; }

        protected GamePlayerEvent(IGamePlayer player)
        {
            Player = player;
        }
    }
}
