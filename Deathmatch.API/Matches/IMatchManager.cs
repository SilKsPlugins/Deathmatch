using OpenMod.API.Ioc;
using System.Collections.Generic;
using Deathmatch.API.Matches.Registrations;

namespace Deathmatch.API.Matches
{
    [Service]
    public interface IMatchManager
    {
        IReadOnlyCollection<IMatchProvider> MatchProviders { get; }

        IReadOnlyCollection<IMatchRegistration> GetMatchRegistrations();

        void AddMatchProvider(IMatchProvider provider);
        void RemoveMatchProvider(IMatchProvider provider);
    }
}
