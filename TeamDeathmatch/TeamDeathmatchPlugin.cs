using Cysharp.Threading.Tasks;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: PluginMetadata("TeamDeathmatch", DisplayName = "Team Deathmatch")]
namespace TeamDeathmatch
{
    public class TeamDeathmatchPlugin : OpenModUnturnedPlugin
    {
        public const string RedSpawnsKey = "redspawns";
        public const string BlueSpawnsKey = "bluespawns";

        private readonly IConfiguration _configuration;

        private readonly List<PlayerSpawn> _redSpawns;
        private readonly List<PlayerSpawn> _blueSpawns;

        public TeamDeathmatchPlugin(IConfiguration configuration,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;

            _redSpawns = new List<PlayerSpawn>();
            _blueSpawns = new List<PlayerSpawn>();
        }

        protected override async UniTask OnLoadAsync()
        {
            await ReloadSpawns();
        }

        protected override UniTask OnUnloadAsync()
        {
            return UniTask.CompletedTask;
        }

        public IReadOnlyCollection<PlayerSpawn> RedSpawns => _redSpawns.AsReadOnly();

        public IReadOnlyCollection<PlayerSpawn> BlueSpawns => _blueSpawns.AsReadOnly();

        public async Task ReloadSpawns()
        {
            async Task LoadList<T>(string key, List<T> list)
            {
                var loadedList = await DataStore.ExistsAsync(key) ? await DataStore.LoadAsync<List<T>>(key) : null;

                loadedList ??= new List<T>();

                list.Clear();
                list.AddRange(loadedList);
            }

            await LoadList(RedSpawnsKey, _redSpawns);
            await LoadList(BlueSpawnsKey, _blueSpawns);
        }

        public IReadOnlyCollection<Loadout> Loadouts =>
            _configuration.GetSection("Loadouts").Get<List<Loadout>>() ??
            new List<Loadout>();
    }
}
