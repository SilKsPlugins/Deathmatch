using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using OpenMod.Core.Commands;
using System;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("clear")]
    [CommandAlias("c")]
    [CommandDescription("Removes all spawns.")]
    [CommandSyntax("<[r]ed/[b]lue>")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsClear : CSpawnsAction
    {
        public CSpawnsClear(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            await SaveSpawns(team, Array.Empty<PlayerSpawn>());

            await PrintAsync(StringLocalizer["commands:spawns:clear:success", new { Team = team.ToString() }]);
        }
    }
}