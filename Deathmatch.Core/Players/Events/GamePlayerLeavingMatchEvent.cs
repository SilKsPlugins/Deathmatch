using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerLeavingMatchEvent : GamePlayerMatchEvent, IGamePlayerLeavingMatchEvent
    {
        public bool IsCancelled { get; set; }

        public GamePlayerLeavingMatchEvent(IGamePlayer player, IMatch match) : base(player, match)
        {
        }
    }
}
