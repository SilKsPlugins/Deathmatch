using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace TeamDeathmatch.Commands
{
    [Command("tdm")]
    [CommandSyntax("<spawns>")]
    [CommandDescription("Manage the Team Deathmatch game mode.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandTDM : UnturnedCommand
    {
        public CommandTDM(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}
