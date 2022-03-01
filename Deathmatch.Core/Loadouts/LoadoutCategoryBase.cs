using Deathmatch.API.Loadouts;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OpenMod.API;
using OpenMod.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public abstract class LoadoutCategoryBase<TLoadout, TItem> : ILoadoutCategory<TLoadout>
        where TItem : Item
        where TLoadout : LoadoutBase<TItem>
    {
        protected LoadoutCategoryBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DataStore = serviceProvider.GetRequiredService<IDataStore>();
            Logger = (ILogger)_serviceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));

            Component = serviceProvider.GetRequiredService<IOpenModComponent>();

            Loadouts = Array.Empty<TLoadout>();
        }

        public abstract string Title { get; }

        public abstract IReadOnlyCollection<string> Aliases { get; }

        private readonly IServiceProvider _serviceProvider;

        protected readonly IDataStore DataStore;
        protected readonly ILogger Logger;

        protected TLoadout[] Loadouts;

        public IOpenModComponent Component { get; }

        protected virtual string DataStoreKey => "loadouts";

        ILoadout? ILoadoutCategory.GetDefaultLoadout() => GetDefaultLoadout();

        IReadOnlyCollection<ILoadout> ILoadoutCategory.GetLoadouts() => GetLoadouts();

        public TLoadout? GetDefaultLoadout()
        {
            return Loadouts.Length > 0 ? Loadouts.RandomElement() : null;
        }

        public IReadOnlyCollection<TLoadout> GetLoadouts()
        {
            return Loadouts;
        }

        public void AddLoadout(TLoadout loadout)
        {
            loadout.ProvideServices(_serviceProvider);

            Loadouts = Loadouts.Append(loadout).ToArray();
        }

        public bool RemoveLoadout(TLoadout loadout)
        {
            var oldLength = Loadouts.Length;

            Loadouts = Loadouts.Where(x => x != loadout).ToArray();

            return Loadouts.Length != oldLength;
        }
        
        public virtual async Task Load()
        {
            if (!await DataStore.ExistsAsync(DataStoreKey))
            {
                Loadouts = Array.Empty<TLoadout>();
                return;
            }

            var loadouts = new List<TLoadout>();
            var pendingLoadouts = await DataStore.LoadAsync<TLoadout[]>(DataStoreKey) ?? Array.Empty<TLoadout>();

            foreach (var loadout in pendingLoadouts)
            {
                try
                {
                    loadout.ProvideServices(_serviceProvider);

                    loadouts.Add(loadout);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error occurred when loading loadout '{LoadoutTitle}'. Skipping this loadout.", loadout.Title);
                }
            }

            Loadouts = loadouts.ToArray();
        }

        public virtual async Task Save()
        {
            await DataStore.SaveAsync(DataStoreKey, Loadouts);
        }
    }
}
