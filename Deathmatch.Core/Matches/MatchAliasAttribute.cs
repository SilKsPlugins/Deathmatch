using System;

namespace Deathmatch.Core.Matches
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MatchAliasAttribute : Attribute
    {
        public string Alias { get; }

        public MatchAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
