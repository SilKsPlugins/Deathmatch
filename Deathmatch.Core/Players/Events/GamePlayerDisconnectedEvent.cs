using Deathmatch.API.Players;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerDisconnectedEvent : GamePlayerEvent, IGamePlayerDisconnectedEvent
    {
        public GamePlayerDisconnectedEvent(IGamePlayer player) : base(player)
        {
        }
    }
}