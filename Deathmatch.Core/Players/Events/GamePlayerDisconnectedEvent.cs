using Deathmatch.API.Players;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerDisconnectedEvent : GamePlayerEvent
    {
        public GamePlayerDisconnectedEvent(IGamePlayer player) : base(player)
        {
        }
    }
}