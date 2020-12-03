using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("clear")]
    [CommandAlias("c")]
    [CommandDescription("Removes all spawns.")]
    [CommandSyntax("<[r]ed/[b]lue>")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsClear : CommandSpawnsAction
    {
        public CommandSpawnsClear(TeamDeathmatchPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            List<PlayerSpawn> spawns = new List<PlayerSpawn>();

            await SaveSpawnsAsync(team, spawns);

            await PrintAsync(StringLocalizer["commands:spawns:clear:success", new { Team = team.ToString() }]);
        }
    }
}