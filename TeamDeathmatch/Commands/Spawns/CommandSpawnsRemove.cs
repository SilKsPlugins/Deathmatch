using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("remove")]
    [CommandAlias("rem")]
    [CommandAlias("r")]
    [CommandAlias("-")]
    [CommandDescription("Remove a spawn.")]
    [CommandSyntax("<[r]ed/[b]lue> <index>")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsRemove : CommandSpawnsAction
    {
        public CommandSpawnsRemove(TeamDeathmatchPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            int index = await Context.Parameters.GetAsync<int>(1);

            // A check which we can call before loading from the disk
            if (index <= 0)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            List<PlayerSpawn> spawns = await LoadSpawnsAsync(team);

            if (index >= spawns.Count)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            spawns.RemoveAt(index);

            await SaveSpawnsAsync(team, spawns);

            await PrintAsync(StringLocalizer["commands:spawns:remove:success", new { Team = team.ToString() }]);
        }
    }
}