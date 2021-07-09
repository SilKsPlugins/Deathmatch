using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Items;

namespace TeamDeathmatch.Loadouts
{
    [Serializable]
    public class TeamLoadout : LoadoutBase
    {
        public List<TeamItem> Items { get; set; }

        public TeamLoadout() : this("", null)
        {
        }

        public TeamLoadout(string title, string? permission) : base(title, permission)
        {
            Items = new List<TeamItem>();
        }

        public override IReadOnlyCollection<Item> GetItems()
        {
            return Items;
        }
    }
}
