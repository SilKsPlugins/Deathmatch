using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;

namespace FreeForAll.Commands.Spawns
{
    [Command("remove")]
    [CommandAlias("rem")]
    [CommandAlias("r")]
    [CommandAlias("-")]
    [CommandDescription("Remove a spawn.")]
    [CommandSyntax("<index>")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsRemove : CommandSpawnsAction
    {
        public CommandSpawnsRemove(FreeForAllPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            int index = await Context.Parameters.GetAsync<int>(0);

            // A check which we can call before loading from the disk
            if (index <= 0)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            List<PlayerSpawn> spawns = await LoadSpawnsAsync();

            if (index >= spawns.Count)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            spawns.RemoveAt(index);

            await SaveSpawnsAsync(spawns);

            await PrintAsync(StringLocalizer["commands:spawns:remove:success"]);
        }
    }
}