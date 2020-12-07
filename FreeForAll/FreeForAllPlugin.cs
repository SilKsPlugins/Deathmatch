using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: PluginMetadata("FreeForAll", DisplayName = "Free For All")]
namespace FreeForAll
{
    public class FreeForAllPlugin : OpenModUnturnedPlugin
    {
        public const string SpawnsKey = "spawns";

        private readonly IConfiguration _configuration;
        private readonly ILoadoutManager _loadoutManager;
        private readonly IDataStore _dataStore;
        private readonly IPermissionRegistry _permissionRegistry;

        private readonly List<PlayerSpawn> _spawns;

        public FreeForAllPlugin(IConfiguration configuration,
            ILoadoutManager loadoutManager,
            IDataStore dataStore,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _loadoutManager = loadoutManager;
            _dataStore = dataStore;
            _permissionRegistry = permissionRegistry;

            _spawns = new List<PlayerSpawn>();
        }

        protected override async UniTask OnLoadAsync()
        {
            await ReloadSpawns();

            var category =
                new LoadoutCategory("FreeForAll", new List<string> {"Free For All", "FFA"}, this, _dataStore);
            await category.LoadLoadouts();

            foreach (var loadout in category.GetLoadouts())
            {
                _permissionRegistry.RegisterPermission(this, loadout.Permission);
            }

            _loadoutManager.AddCategory(category);
        }

        protected override UniTask OnUnloadAsync()
        {
            return UniTask.CompletedTask;
        }

        public IReadOnlyCollection<PlayerSpawn> Spawns => _spawns.AsReadOnly();

        public async Task ReloadSpawns()
        {
            async Task LoadList<T>(string key, List<T> list)
            {
                var loadedList = await DataStore.ExistsAsync(key) ? await DataStore.LoadAsync<List<T>>(key) : null;

                loadedList ??= new List<T>();

                list.Clear();
                list.AddRange(loadedList);
            }

            await LoadList(SpawnsKey, _spawns);
        }

        public IReadOnlyCollection<Loadout> Loadouts =>
            _configuration.GetSection("Loadouts").Get<List<Loadout>>() ??
            new List<Loadout>();
    }
}
