using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerJoiningMatchEvent : GamePlayerMatchEvent, IGamePlayerJoiningMatchEvent
    {
        public bool IsCancelled { get; set; }

        public GamePlayerJoiningMatchEvent(IGamePlayer player, IMatch match) : base(player, match)
        {
        }
    }
}
