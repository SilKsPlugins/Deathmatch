using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;

namespace Deathmatch.Core.Matches.Events
{
    public class MatchStartedEvent : MatchEvent, IMatchStartedEvent
    {
        public MatchStartedEvent(IMatch match) : base(match)
        {
        }
    }
}
