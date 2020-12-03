using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;

namespace FreeForAll.Commands.Spawns
{
    [Command("clear")]
    [CommandAlias("c")]
    [CommandDescription("Removes all spawns.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsClear : CommandSpawnsAction
    {
        public CommandSpawnsClear(FreeForAllPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            List<PlayerSpawn> spawns = new List<PlayerSpawn>();

            await SaveSpawnsAsync(spawns);

            await PrintAsync(StringLocalizer["commands:spawns:clear:success"]);
        }
    }
}