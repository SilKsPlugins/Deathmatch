using OpenMod.API.Ioc;
using System.Collections.Generic;

namespace Deathmatch.API.Matches
{
    [Service]
    public interface IMatchManager
    {
        IReadOnlyCollection<IMatchProvider> MatchProviders { get; }

        IReadOnlyCollection<IMatchRegistration> GetMatchRegistrations();
        IReadOnlyCollection<IMatchRegistration> GetEnabledMatchRegistrations();
        IMatchRegistration? GetMatchRegistration(string title);

        void AddMatchProvider(IMatchProvider provider);
        void RemoveMatchProvider(IMatchProvider provider);
    }
}
