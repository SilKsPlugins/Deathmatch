using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using System;
using System.Collections.Generic;

namespace FreeForAll.Commands.Spawns
{
    public abstract class CommandSpawnsAction : UnturnedCommand
    {
        protected readonly FreeForAllPlugin Plugin;
        protected readonly IStringLocalizer StringLocalizer;

        protected CommandSpawnsAction(FreeForAllPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Plugin = plugin;
            StringLocalizer = stringLocalizer;
        }

        public string SpawnsKey => FreeForAllPlugin.SpawnsKey;

        protected async UniTask<List<PlayerSpawn>> LoadSpawnsAsync()
        {
            var list = await Plugin.DataStore.ExistsAsync(SpawnsKey)
                ? await Plugin.DataStore.LoadAsync<List<PlayerSpawn>>(SpawnsKey)
                : null;

            return list ?? new List<PlayerSpawn>();
        }

        protected async UniTask SaveSpawnsAsync(List<PlayerSpawn> spawns)
        {
            await Plugin.DataStore.SaveAsync(SpawnsKey, spawns);

            await Plugin.ReloadSpawns();
        }
    }
}
