using Deathmatch.API.Matches;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Prioritization;
using System;
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
            return MatchProviders.SelectMany(x => x.GetMatchRegistrations()).OrderBy(x => x.Priority, _priorityComparer)
                .ToList().AsReadOnly();
        }

        public IReadOnlyCollection<IMatchRegistration> GetEnabledMatchRegistrations()
        {
            return MatchProviders.SelectMany(x => x.GetMatchRegistrations()).Where(x => x.Enabled)
                .OrderBy(x => x.Priority, _priorityComparer).ToList().AsReadOnly();
        }

        public IMatchRegistration? GetMatchRegistration(string title)
        {
            var registrations = GetEnabledMatchRegistrations();

            IMatchRegistration? titleMatch = null;
            IMatchRegistration? aliasMatch = null;

            foreach (var registration in registrations)
            {
                if (title.Equals(registration.Title, StringComparison.OrdinalIgnoreCase))
                {
                    titleMatch = registration;
                    break;
                }

                if (registration.Aliases.Any(x => title.Equals(x, StringComparison.OrdinalIgnoreCase)))
                {
                    aliasMatch = registration;
                }
            }

            return titleMatch ?? aliasMatch;
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
