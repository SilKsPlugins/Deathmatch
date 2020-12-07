using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerDisconnectedEvent : GamePlayerEvent, IGamePlayerDisconnectedEvent
    {
        public GamePlayerDisconnectedEvent(IGamePlayer player) : base(player)
        {
        }
    }
}