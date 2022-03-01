using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Persistence;
using OpenMod.API.Prioritization;
using OpenMod.Core.Helpers;
using OpenMod.Core.Plugins.Events;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public sealed class LoadoutSelector : ILoadoutSelector, IDisposable,
        IInstanceEventListener<PluginLoadedEvent>
    {
        public class CategorySelections
        {
            public string CategoryTitle { get; set; } = "";

            public Dictionary<ulong, string> Selections = new();
        }

        private IDataStore? _dataStore;
        private readonly CancellationTokenSource _cts;
        private List<CategorySelections> _categories;

        private bool _isDirty;

        private const string DataStoreKey = "loadoutselections";

        public LoadoutSelector()
        {
            _cts = new();
            _categories = new();
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        public async UniTask HandleEventAsync(object? sender, PluginLoadedEvent @event)
        {
            if (@event.Plugin.GetType() != typeof(DeathmatchPlugin))
            {
                return;
            }

            _dataStore = @event.Plugin.DataStore;

            _categories = await Load();

            AsyncHelper.Schedule($"{nameof(LoadoutSelector)} - {nameof(SaveLoop)}", SaveLoop);
        }

        private async Task SaveLoop()
        {
            try
            {
                var cancellationToken = _cts.Token;

                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(60000, cancellationToken);

                    if (_dataStore != null && _isDirty)
                    {
                        _isDirty = false;
                        await _dataStore.SaveAsync(DataStoreKey, _categories);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async Task<List<CategorySelections>> Load()
        {
            if (_dataStore == null)
            {
                throw new Exception("No data store to load spawns");
            }

            return await _dataStore.LoadAsync<List<CategorySelections>>(DataStoreKey) ?? new();
        }

        public ILoadout? GetSelectedLoadout(IGamePlayer player, ILoadoutCategory category)
        {
            var categorySelections = _categories.FirstOrDefault(x => x.CategoryTitle.Equals(category.Title));

            if (categorySelections == null || !categorySelections.Selections.TryGetValue(player.SteamId.m_SteamID, out var selection))
            {
                return null;
            }
            
            return category.GetLoadout(selection);
        }

        public void SetSelectedLoadout(IGamePlayer player, ILoadoutCategory category, ILoadout loadout)
        {
            var categorySelections = _categories.FirstOrDefault(x => x.CategoryTitle.Equals(category.Title));

            if (categorySelections == null)
            {
                categorySelections = new CategorySelections {CategoryTitle = category.Title};
                _categories.Add(categorySelections);
            }

            categorySelections.Selections[player.SteamId.m_SteamID] = loadout.Title;

            _isDirty = true;
        }
    }
}
