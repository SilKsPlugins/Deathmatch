using System;

namespace Deathmatch.Core.Matches
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MatchDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public MatchDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
