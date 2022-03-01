using Cysharp.Threading.Tasks;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;

namespace FreeForAll.Commands.Spawns
{
    [Command("remove")]
    [CommandAlias("rem")]
    [CommandAlias("r")]
    [CommandAlias("-")]
    [CommandDescription("Remove a spawn.")]
    [CommandSyntax("<index>")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsRemove : CSpawnsAction
    {
        public CSpawnsRemove(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var index = await Context.Parameters.GetAsync<int>(0);

            // A check which we can call before loading from the disk
            if (index <= 0)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            var spawns = GetSpawns();

            if (index >= spawns.Count)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:remove:index_out_of_bounds"]);
            }

            spawns.RemoveAt(index);

            await SaveSpawns(spawns);

            await PrintAsync(StringLocalizer["commands:spawns:remove:success"]);
        }
    }
}