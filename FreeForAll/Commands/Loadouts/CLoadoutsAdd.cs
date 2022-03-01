using Deathmatch.Core.Commands.Loadouts.Base;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using FreeForAll.Loadouts;
using OpenMod.Core.Commands;
using System;

namespace FreeForAll.Commands.Loadouts
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Adds a new Free For All loadout.")]
    [CommandSyntax("<title>")]
    [CommandParent(typeof(CLoadouts))]
    public class CLoadoutsAdd : AddLoadoutCommand<FFALoadoutCategory, BasicLoadout, Item>
    {
        public CLoadoutsAdd(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override BasicLoadout CreateLoadout(string title, string permission)
        {
            return new(title, permission);
        }
    }
}