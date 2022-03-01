using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using System;
using System.Collections.Generic;

namespace FreeForAll.Loadouts
{
    public class FFALoadoutCategory : LoadoutCategoryBase<BasicLoadout, Item>
    {
        public FFALoadoutCategory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string Title => "Free For All";

        public override IReadOnlyCollection<string> Aliases => new[] {"FreeForAll", "FFA"};
    }
}
