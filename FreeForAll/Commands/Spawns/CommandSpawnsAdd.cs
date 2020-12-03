using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;

namespace FreeForAll.Commands.Spawns
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Add a spawn.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsAdd : CommandSpawnsAction
    {
        public CommandSpawnsAdd(FreeForAllPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            List<PlayerSpawn> spawns = await LoadSpawnsAsync();

            var spawn = new PlayerSpawn((UnturnedUser)Context.Actor);

            spawns.Add(spawn);

            await SaveSpawnsAsync(spawns);

            await PrintAsync(StringLocalizer["commands:spawns:add:success"]);
        }
    }
}