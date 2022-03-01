using Deathmatch.Core.Commands.Loadouts.Base;
using OpenMod.Core.Commands;
using System;
using TeamDeathmatch.Items;
using TeamDeathmatch.Loadouts;

namespace TeamDeathmatch.Commands.Loadouts
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Adds a new Team Deathmatch loadout.")]
    [CommandSyntax("<title>")]
    [CommandParent(typeof(CLoadouts))]
    public class CLoadoutsAdd : AddLoadoutCommand<TDMLoadoutCategory, TeamLoadout, TeamItem>
    {
        public CLoadoutsAdd(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override TeamLoadout CreateLoadout(string title, string permission)
        {
            return new(title, permission);
        }
    }
}