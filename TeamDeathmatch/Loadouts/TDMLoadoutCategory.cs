using Deathmatch.Core.Loadouts;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Items;

namespace TeamDeathmatch.Loadouts
{
    public class TDMLoadoutCategory : LoadoutCategoryBase<TeamLoadout, TeamItem>
    {
        public TDMLoadoutCategory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string Title => "Team Deathmatch";

        public override IReadOnlyCollection<string> Aliases => new[] {"TeamDeathmatch", "TDM"};
    }
}
