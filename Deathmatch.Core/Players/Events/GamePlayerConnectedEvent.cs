using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerConnectedEvent : GamePlayerEvent, IGamePlayerConnectedEvent
    {
        public GamePlayerConnectedEvent(IGamePlayer player) : base(player)
        {
        }
    }
}
