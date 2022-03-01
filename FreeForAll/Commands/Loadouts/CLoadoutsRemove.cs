using Deathmatch.Core.Commands.Loadouts.Base;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using FreeForAll.Loadouts;
using OpenMod.Core.Commands;
using System;

namespace FreeForAll.Commands.Loadouts
{
    [Command("remove")]
    [CommandAlias("rem")]
    [CommandAlias("r")]
    [CommandAlias("-")]
    [CommandDescription("Removes a Free For All loadout.")]
    [CommandSyntax("<title>")]
    [CommandParent(typeof(CLoadouts))]
    public class CLoadoutsRemove : RemoveLoadoutCommand<FFALoadoutCategory, BasicLoadout, Item>
    {
        public CLoadoutsRemove(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}