using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using System;

namespace FreeForAll.Commands.Spawns
{
    [Command("list")]
    [CommandAlias("l")]
    [CommandDescription("Lists the spawns.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsList : CSpawnsAction
    {
        public CSpawnsList(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var spawns = GetSpawns();

            var localizedSpawns = new string[spawns.Count];

            for (var i = 0; i < localizedSpawns.Length; i++)
            {
                localizedSpawns[i] = StringLocalizer["commands:spawns:list:element",
                    new { I = i, spawns[i].X, spawns[i].Y, spawns[i].Z }];
            }

            var list = string.Join(StringLocalizer["commands:spawns:list:delimiter"], localizedSpawns);

            var output = StringLocalizer["commands:spawns:list:header", new { List = list }];

            await PrintAsync(output);
        }
    }
}