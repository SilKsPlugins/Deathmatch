using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using OpenMod.Core.Commands;
using System;

namespace FreeForAll.Commands.Spawns
{
    [Command("clear")]
    [CommandAlias("c")]
    [CommandDescription("Removes all spawns.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsClear : CSpawnsAction
    {
        public CSpawnsClear(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            await SaveSpawns(Array.Empty<PlayerSpawn>());

            await PrintAsync(StringLocalizer["commands:spawns:clear:success"]);
        }
    }
}