using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;

namespace FreeForAll.Commands.Spawns
{
    [Command("spawns")]
    [CommandAlias("spawn")]
    [CommandDescription("Manages FFA spawns.")]
    [CommandSyntax("<[a]dd/[r]emove/[l]ist/[c]lear>")]
    [CommandParent(typeof(CommandFFA))]
    public class CommandSpawns : UnturnedCommand
    {
        public CommandSpawns(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}
