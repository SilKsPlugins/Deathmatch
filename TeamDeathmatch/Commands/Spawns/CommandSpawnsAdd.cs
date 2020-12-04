using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Add a spawn.")]
    [CommandSyntax("<[r]ed/[b]lue>")]
    [CommandParent(typeof(CommandSpawns))]
    public class CommandSpawnsAdd : CommandSpawnsAction
    {
        public CommandSpawnsAdd(TeamDeathmatchPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(plugin, stringLocalizer, serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            List<PlayerSpawn> spawns = await LoadSpawnsAsync(team);

            var spawn = new PlayerSpawn((UnturnedUser)Context.Actor);

            spawns.Add(spawn);

            await SaveSpawnsAsync(team, spawns);

            await PrintAsync(StringLocalizer["commands:spawns:add:success", new { Team = team.ToString() }]);
        }
    }
}