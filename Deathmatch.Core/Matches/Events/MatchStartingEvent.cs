using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;

namespace Deathmatch.Core.Matches.Events
{
    public class MatchStartingEvent : MatchEvent, IMatchStartingEvent
    {
        public bool IsCancelled { get; set; }

        public MatchStartingEvent(IMatch match) : base(match)
        {
        }
    }
}
