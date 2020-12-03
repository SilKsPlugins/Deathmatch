using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("spawns")]
    [CommandAlias("spawn")]
    [CommandDescription("Manages TDM spawns.")]
    [CommandSyntax("<[a]dd/[r]emove/[l]ist/[c]lear> <[r]ed/[b]lue>")]
    [CommandParent(typeof(CommandTDM))]
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
