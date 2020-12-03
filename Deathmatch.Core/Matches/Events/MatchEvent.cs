using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;
using OpenMod.Core.Eventing;

namespace Deathmatch.Core.Matches.Events
{
    public abstract class MatchEvent : Event, IMatchEvent
    {
        public IMatch Match { get; }

        protected MatchEvent(IMatch match)
        {
            Match = match;
        }
    }
}
