using Deathmatch.API.Matches;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Deathmatch.Core.Matches
{
    public class AssemblyMatchProvider : IMatchProvider
    {
        private readonly List<IMatchRegistration> _matchRegistrations;

        public AssemblyMatchProvider(Assembly assembly, ILogger<AssemblyMatchProvider> logger)
        {
            _matchRegistrations = new List<IMatchRegistration>();

            foreach (var type in assembly.FindAllTypes().Where(typeof(IMatch).IsAssignableFrom))
            {
                if (type == null) continue;

                try
                {
                    _matchRegistrations.Add(new RegisteredMatch(type));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Invalid {nameof(IMatch)} implementation by type {type.FullName}");
                }
            }
        }

        public IReadOnlyCollection<IMatchRegistration> GetMatchRegistrations() => _matchRegistrations.AsReadOnly();
    }
}
