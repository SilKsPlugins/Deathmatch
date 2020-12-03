using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;

namespace FreeForAll.Commands.Spawns
{
    [Command("list")]
    [CommandAlias("l")]
    [CommandDescription("Lists the spawns.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsList : CommandSpawnsAction
    {
        public CommandSpawnsList(FreeForAllPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            List<PlayerSpawn> spawns = await LoadSpawnsAsync();

            string[] localizedSpawns = new string[spawns.Count];

            for (int i = 0; i < localizedSpawns.Length; i++)
            {
                localizedSpawns[i] = StringLocalizer["commands:spawns:list:element",
                    new { I = i, spawns[i].X, spawns[i].Y, spawns[i].Z }];
            }

            string list = string.Join(StringLocalizer["commands:spawns:list:delimiter"], localizedSpawns);

            string output = StringLocalizer["commands:spawns:list:header", new { List = list }];

            await PrintAsync(output);
        }
    }
}