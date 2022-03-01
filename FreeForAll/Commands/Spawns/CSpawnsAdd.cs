using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;

namespace FreeForAll.Commands.Spawns
{
    [Command("add")]
    [CommandAlias("a")]
    [CommandAlias("+")]
    [CommandDescription("Add a spawn.")]
    [CommandSyntax("")]
    [CommandParent(typeof(CSpawns))]
    public class CSpawnsAdd : CSpawnsAction
    {
        public CSpawnsAdd(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var spawns = GetSpawns();

            var spawn = new PlayerSpawn((UnturnedUser)Context.Actor);

            spawns.Add(spawn);

            await SaveSpawns(spawns);

            await PrintAsync(StringLocalizer["commands:spawns:add:success"]);
        }
    }
}