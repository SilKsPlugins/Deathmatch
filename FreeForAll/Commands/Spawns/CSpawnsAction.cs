using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeForAll.Commands.Spawns
{
    public abstract class CSpawnsAction : UnturnedCommand
    {
        protected readonly IStringLocalizer StringLocalizer;
        protected readonly SpawnDirectory SpawnDirectory;

        protected CSpawnsAction(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            StringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer>();
            SpawnDirectory = serviceProvider.GetRequiredService<SpawnDirectory>();
        }

        public string SpawnsKey => FreeForAllPlugin.SpawnsKey;

        protected List<PlayerSpawn> GetSpawns()
        {
            return SpawnDirectory.Spawns.ToList();
        }

        protected async UniTask SaveSpawns(IEnumerable<PlayerSpawn> spawns)
        {
            await SpawnDirectory.SaveSpawns(spawns);
        }
    }
}
