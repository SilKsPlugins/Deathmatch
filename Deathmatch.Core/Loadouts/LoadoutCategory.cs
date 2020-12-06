using Deathmatch.API.Loadouts;
using OpenMod.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public class LoadoutCategory : ILoadoutCategory
    {
        public string Title { get; }

        public IReadOnlyCollection<string> Aliases { get; }

        private readonly List<ILoadout> _loadouts;
        private readonly IDataStore _dataStore;

        public LoadoutCategory(string title, IReadOnlyCollection<string> aliases, IDataStore dataStore)
        {
            Title = title;
            Aliases = aliases ?? new List<string>();
            _loadouts = new List<ILoadout>();

            _dataStore = dataStore;
        }

        public LoadoutCategory(string title, IReadOnlyCollection<string> aliases, List<ILoadout> loadouts, IDataStore dataStore)
        {
            Title = title;
            Aliases = aliases ?? new List<string>();
            _loadouts = loadouts ?? new List<ILoadout>();

            _dataStore = dataStore;
        }

        public ILoadout GetLoadout(string title) =>
            _loadouts.FirstOrDefault(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyCollection<ILoadout> GetLoadouts() => _loadouts.AsReadOnly();

        public Task SaveLoadouts() => _dataStore.SaveAsync(Title, _loadouts.OfType<Loadout>().ToList());

        public void AddLoadout(ILoadout loadout)
        {
            if (GetLoadout(loadout.Title) != null)
                throw new ArgumentException("Loadout with given title already exists", nameof(loadout));

            _loadouts.Add(loadout);
        }

        public bool RemoveLoadout(ILoadout loadout)
        {
            return _loadouts.Remove(loadout);
        }
    }
}
