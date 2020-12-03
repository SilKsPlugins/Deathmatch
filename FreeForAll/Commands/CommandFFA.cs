using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace FreeForAll.Commands
{
    [Command("ffa")]
    [CommandSyntax("<spawns>")]
    [CommandDescription("Manage the Free For All game mode.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandFFA : UnturnedCommand
    {
        public CommandFFA(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}
