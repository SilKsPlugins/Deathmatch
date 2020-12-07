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
using System.Linq;
using System.Threading.Tasks;

[assembly: PluginMetadata("TeamDeathmatch", DisplayName = "Team Deathmatch")]
namespace TeamDeathmatch
{
    public class TeamDeathmatchPlugin : OpenModUnturnedPlugin
    {
        public const string RedSpawnsKey = "redspawns";
        public const string BlueSpawnsKey = "bluespawns";

        private readonly IConfiguration _configuration;
        private readonly ILoadoutManager _loadoutManager;
        private readonly IDataStore _dataStore;
        private readonly IPermissionRegistry _permissionRegistry;

        private readonly List<PlayerSpawn> _redSpawns;
        private readonly List<PlayerSpawn> _blueSpawns;

        public TeamDeathmatchPlugin(IConfiguration configuration,
            ILoadoutManager loadoutManager,
            IDataStore dataStore,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _loadoutManager = loadoutManager;
            _dataStore = dataStore;
            _permissionRegistry = permissionRegistry;

            _redSpawns = new List<PlayerSpawn>();
            _blueSpawns = new List<PlayerSpawn>();
        }

        protected override async UniTask OnLoadAsync()
        {
            await ReloadSpawns();

            var category = new LoadoutCategory("Team Deathmatch", new List<string> {"TeamDeathmatch", "TDM"}, this,
                _dataStore);
            await category.LoadLoadouts();

            foreach (var loadout in category.GetLoadouts().OfType<Loadout>())
            {
                _permissionRegistry.RegisterPermission(this, loadout.GetPermissionWithoutComponent());
            }

            _loadoutManager.AddCategory(category);
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
