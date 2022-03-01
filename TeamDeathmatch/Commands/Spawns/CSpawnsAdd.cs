using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Linq;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Add a spawn.")]
    [CommandSyntax("<[r]ed/[b]lue>")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsAdd : CSpawnsAction
    {
        public CSpawnsAdd(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            var spawns = GetSpawns(team);
            
            var spawn = new PlayerSpawn((UnturnedUser)Context.Actor);

            spawns.Add(spawn);

            await SaveSpawns(team, spawns);

            await PrintAsync(StringLocalizer["commands:spawns:add:success", new { Team = team.ToString() }]);
        }
    }
}