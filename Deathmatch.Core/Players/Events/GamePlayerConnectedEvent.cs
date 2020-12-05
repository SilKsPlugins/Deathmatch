using Deathmatch.API.Players;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerConnectedEvent : GamePlayerEvent
    {
        public GamePlayerConnectedEvent(IGamePlayer player) : base(player)
        {
        }
    }
}
