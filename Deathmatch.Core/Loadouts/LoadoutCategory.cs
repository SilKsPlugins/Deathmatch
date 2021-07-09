using Deathmatch.API.Loadouts;
using OpenMod.API;
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

        public IOpenModComponent Component { get; }

        protected List<ILoadout> Loadouts;
        protected readonly IDataStore DataStore;

        protected const string DataStoreKey = "loadouts";

        public LoadoutCategory(string title, IReadOnlyCollection<string>? aliases, IOpenModComponent component,
            IDataStore dataStore, List<ILoadout>? loadouts = null)
        {
            Title = title;
            Aliases = aliases ?? new List<string>();

            Component = component;
            DataStore = dataStore;

            Loadouts = loadouts ?? new List<ILoadout>();
        }

        public IReadOnlyCollection<ILoadout> GetLoadouts() => Loadouts.AsReadOnly();

        public virtual async Task LoadLoadouts()
        {
            var loadouts = new List<ILoadout>();

            if (await DataStore.ExistsAsync(DataStoreKey))
            {
                loadouts.AddRange(await DataStore.LoadAsync<List<Loadout>>(DataStoreKey) ?? new List<Loadout>());
            }

            Loadouts = loadouts;
        }

        public virtual async Task SaveLoadouts()
        {
            await DataStore.SaveAsync(DataStoreKey, Loadouts.OfType<Loadout>().ToList());
        }

        public virtual void AddLoadout(ILoadout loadout)
        {
            if (this.GetLoadout(loadout.Title) != null)
            {
                throw new ArgumentException("Loadout with given title already exists", nameof(loadout));
            }

            Loadouts.Add(loadout);
        }

        public virtual bool RemoveLoadout(ILoadout loadout)
        {
            return Loadouts.Remove(loadout);
        }
    }
}
