using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;

namespace Deathmatch.Core.Matches.Events
{
    public class MatchEndedEvent : MatchEvent, IMatchEndedEvent
    {
        public MatchEndedEvent(IMatch match) : base(match)
        {
        }
    }
}
