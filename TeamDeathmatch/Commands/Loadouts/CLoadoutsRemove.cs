using Deathmatch.Core.Commands.Loadouts.Base;
using OpenMod.Core.Commands;
using System;
using TeamDeathmatch.Items;
using TeamDeathmatch.Loadouts;

namespace TeamDeathmatch.Commands.Loadouts
{
    [Command("remove")]
    [CommandAlias("rem")]
    [CommandAlias("r")]
    [CommandAlias("-")]
    [CommandDescription("Removes a Team Deathmatch loadout.")]
    [CommandSyntax("<title>")]
    [CommandParent(typeof(CLoadouts))]
    public class CLoadoutsRemove : RemoveLoadoutCommand<TDMLoadoutCategory, TeamLoadout, TeamItem>
    {
        public CLoadoutsRemove(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}