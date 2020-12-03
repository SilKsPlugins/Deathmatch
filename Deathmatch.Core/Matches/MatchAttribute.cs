using OpenMod.API.Prioritization;
using System;

namespace Deathmatch.Core.Matches
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MatchAttribute : PriorityAttribute
    {
        public string Title { get; set; }

        public MatchAttribute(string title)
        {
            Title = title;
        }
    }
}
