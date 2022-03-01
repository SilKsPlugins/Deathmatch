using Deathmatch.API.Matches.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.API.Matches
{
    public static class MatchManagerExtensions
    {
        public static IReadOnlyCollection<IMatchRegistration> GetEnabledMatchRegistrations(
            this IMatchManager matchManager)
        {
            return matchManager.GetMatchRegistrations().Where(x => x.Enabled).ToList();
        }

        public static IMatchRegistration? GetMatchRegistration(this IMatchManager matchManager, string title)
        {
            var registrations = matchManager.GetEnabledMatchRegistrations();

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
    }
}
