using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace FreeForAll.Commands
{
    [Command("ffa")]
    [CommandAlias("freeforall")]
    [CommandSyntax("<spawns | loadouts>")]
    [CommandDescription("Manage the Free For All game mode.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CFreeForAll : UnturnedCommand
    {
        public CFreeForAll(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}
