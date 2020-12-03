using Deathmatch.API.Matches;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Deathmatch.Core.Matches
{
    public class RegisteredMatch : RegisteredMatchInfo, IMatchRegistration
    {
        public Type MatchType { get; }
        
        public IMatch Instantiate(IServiceProvider serviceProvider)
        {
            return (IMatch)ActivatorUtilities.CreateInstance(serviceProvider, MatchType);
        }

        public RegisteredMatch(Type type)
        {
            if (!typeof(IMatch).IsAssignableFrom(type))
                throw new ArgumentException($"Argument is not assignable from {nameof(IMatch)}", nameof(type));

            Id = type.FullName;

            var match = type.GetCustomAttribute<MatchAttribute>();
            var matchDescription = type.GetCustomAttribute<MatchDescriptionAttribute>();
            var matchAliases = type.GetCustomAttributes<MatchAliasAttribute>();

            if (match == null)
                throw new ArgumentException($"Type argument has no {nameof(MatchAttribute)} attribute defined", nameof(type));

            if (string.IsNullOrEmpty(match.Title))
                throw new ArgumentException($"Type argument's {nameof(MatchAttribute)} has null or empty Title property", nameof(type));

            MatchType = type;

            Priority = match.Priority;
            Title = match.Title;
            Description = matchDescription?.Description;
            Aliases = matchAliases.Select(x => x.Alias).Where(x => x != null).ToList();
        }
    }
}
