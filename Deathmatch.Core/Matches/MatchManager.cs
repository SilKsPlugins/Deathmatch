using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Registrations;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Prioritization;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Matches
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class MatchManager : IMatchManager
    {
        private readonly List<IMatchProvider> _matchProviders;
        private readonly PriorityComparer _priorityComparer;

        public MatchManager()
        {
            _matchProviders = new List<IMatchProvider>();
            _priorityComparer = new PriorityComparer(PriortyComparisonMode.LowestFirst);
        }

        public IReadOnlyCollection<IMatchProvider> MatchProviders => _matchProviders.AsReadOnly();

        public IReadOnlyCollection<IMatchRegistration> GetMatchRegistrations()
        {
            return MatchProviders.SelectMany(x => x.GetMatchRegistrations())
                .OrderBy(x => x.Priority, _priorityComparer)
                .ToList();
        }

        public void AddMatchProvider(IMatchProvider provider)
        {
            _matchProviders.Add(provider);
        }

        public void RemoveMatchProvider(IMatchProvider provider)
        {
            _matchProviders.Remove(provider);
        }
    }
}
