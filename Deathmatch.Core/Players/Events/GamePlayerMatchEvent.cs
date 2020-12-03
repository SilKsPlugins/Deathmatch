using Deathmatch.API.Matches;
using Deathmatch.API.Players;

namespace Deathmatch.Core.Players.Events
{
    public abstract class GamePlayerMatchEvent : GamePlayerEvent
    {
        public IMatch Match { get; }

        protected GamePlayerMatchEvent(IGamePlayer player, IMatch match) : base(player)
        {
            Match = match;
        }
    }
}
