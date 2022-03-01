using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Persistence;
using OpenMod.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Spawns
{
    public class SpawnDirectory
    {
        private readonly IDataStore _dataStore;

        private List<PlayerSpawn> _spawns = new();

        public SpawnDirectory(IServiceProvider serviceProvider)
        {
            _dataStore = serviceProvider.GetRequiredService<IDataStore>();

            AsyncHelper.RunSync(LoadSpawns);
        }

        protected virtual string DataStoreKey => "spawns";

        public IReadOnlyCollection<PlayerSpawn> Spawns => _spawns.AsReadOnly();

        public async Task LoadSpawns()
        {
            var loadedList = await _dataStore.ExistsAsync(DataStoreKey) ? await _dataStore.LoadAsync<List<PlayerSpawn>>(DataStoreKey) : null;

            _spawns = loadedList ?? new();
        }

        public async Task SaveSpawns(IEnumerable<PlayerSpawn> spawns)
        {
            var playerSpawns = spawns.ToList();

            await _dataStore.SaveAsync(DataStoreKey, playerSpawns);

            _spawns = playerSpawns;
        }
    }
}
