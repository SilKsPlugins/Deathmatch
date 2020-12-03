using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerLeftMatchEvent : GamePlayerMatchEvent, IGamePlayerLeftMatchEvent
    {
        public GamePlayerLeftMatchEvent(IGamePlayer player, IMatch match) : base(player, match)
        {
        }
    }
}
