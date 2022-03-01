using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace TeamDeathmatch.Commands
{
    [Command("tdm")]
    [CommandAlias("teamdeathmatch")]
    [CommandSyntax("<spawns | loadouts>")]
    [CommandDescription("Manage the Team Deathmatch game mode.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTeamDeathmatch : UnturnedCommand
    {
        public CTeamDeathmatch(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}
