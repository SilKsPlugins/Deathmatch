using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using System;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    [Command("list")]
    [CommandAlias("l")]
    [CommandDescription("Lists the spawns.")]
    [CommandSyntax("<[r]ed/[b]lue>")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsList : CSpawnsAction
    {
        public CSpawnsList(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync(Team team)
        {
            var spawns = GetSpawns(team);

            var localizedSpawns = new string[spawns.Count];

            for (var i = 0; i < localizedSpawns.Length; i++)
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