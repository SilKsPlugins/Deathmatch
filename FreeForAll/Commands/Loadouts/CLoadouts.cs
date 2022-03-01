using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;

namespace FreeForAll.Commands.Loadouts
{
    [Command("loadouts")]
    [CommandAlias("loadout")]
    [CommandDescription("Manages Team Deathmatch loadouts.")]
    [CommandSyntax("<[a]dd/[r]emove> <title>")]
    [CommandParent(typeof(CFreeForAll))]
    public class CLoadouts : UnturnedCommand
    {
        public CLoadouts(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}