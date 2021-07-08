using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;

namespace Deathmatch.Core.Matches.Events
{
    public class MatchEndingEvent : MatchEvent, IMatchEndingEvent
    {
        public MatchEndingEvent(IMatch match) : base(match)
        {
        }
    }
}
