using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Common.Helpers;
using OpenMod.Core.Helpers;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public sealed class LoadoutSelector : ILoadoutSelector,
        IInstanceEventListener<IGamePlayerConnectedEvent>,
        IInstanceEventListener<IGamePlayerDisconnectedEvent>
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IUserDataStore _userDataStore;
        private readonly ILogger<LoadoutSelector> _logger;

        private readonly Dictionary<IGamePlayer, List<LoadoutSelection>> _loadoutSelections;

        public LoadoutSelector(ILoadoutManager loadoutManager,
            IUserDataStore userDataStore,
            IGamePlayerManager playerManager,
            ILogger<LoadoutSelector> logger)
        {
            _loadoutManager = loadoutManager;
            _userDataStore = userDataStore;
            _logger = logger;

            _loadoutSelections = new Dictionary<IGamePlayer, List<LoadoutSelection>>();

            AsyncHelper.RunSync(async () =>
            {
                var existingPlayers = playerManager.GetPlayers();

                foreach (var user in existingPlayers)
                {
                    await LoadPlayer(user);
                }
            });
        }

        public ILoadout? GetLoadout(IGamePlayer player, string category)
        {
            var loadoutCategory = _loadoutManager.GetCategory(category);

            if (loadoutCategory == null)
            {
                return null;
            }

            var loadoutTitle = _loadoutSelections[player]
                .FirstOrDefault(x => x.GameMode.Equals(loadoutCategory.Title, StringComparison.OrdinalIgnoreCase))
                ?.Loadout;

            return loadoutTitle == null ? null : loadoutCategory.GetLoadout(loadoutTitle);
        }

        public async Task SetLoadout(IGamePlayer player, string category, string loadout)
        {
            var loadoutCategory = _loadoutManager.GetCategory(category);

            if (loadoutCategory == null) return;

            var selections = _loadoutSelections[player];

            var selection =
                selections.FirstOrDefault(x => x.GameMode.Equals(loadoutCategory.Title, StringComparison.OrdinalIgnoreCase));

            if (selection == null)
            {
                selection = new LoadoutSelection()
                {
                    GameMode = loadoutCategory.Title,
                    Loadout = loadout
                };

                selections.Add(selection);
            }
            else
            {
                selection.Loadout = loadout;
            }

            await _userDataStore.SetUserDataAsync(player.User.Id, player.User.Type, LoadoutSelectionsKey,
                selections);
        }

        private const string LoadoutSelectionsKey = "LoadoutSelections";

        private async Task LoadPlayer(IGamePlayer player)
        {
            var userData =
                await _userDataStore.GetUserDataAsync<object>(player.User.Id, player.User.Type,
                    LoadoutSelectionsKey);

            List<object>? selections = null;

            if (userData is List<object> objects)
            {
                selections = objects;
            }
            else if (userData is List<LoadoutSelection> other)
            {
                selections = other.Cast<object>().ToList();
            }

            var loaded = new List<LoadoutSelection>();

            if (selections != null)
            {
                loaded.AddRange(selections.OfType<LoadoutSelection>());

                loaded.AddRange(selections.OfType<Dictionary<object, object>>()
                    .Select(selection => selection.ToObject<LoadoutSelection>()).Where(parsed => parsed != null));
            }

            if (!_loadoutSelections.ContainsKey(player))
            {
                _loadoutSelections.Add(player, loaded);
            }
            else
            {
                _loadoutSelections[player] = loaded;
            }
        }

        private async Task SavePlayer(IGamePlayer player)
        {
            if (_loadoutSelections.ContainsKey(player))
            {
                await _userDataStore.SetUserDataAsync(player.User.Id, player.User.Type, LoadoutSelectionsKey,
                    _loadoutSelections[player]);

                _loadoutSelections.Remove(player);
            }
        }

        public async UniTask HandleEventAsync(object? sender, IGamePlayerConnectedEvent @event)
        {
            await LoadPlayer(@event.Player);
        }

        public async UniTask HandleEventAsync(object? sender, IGamePlayerDisconnectedEvent @event)
        {
            await SavePlayer(@event.Player);
        }
    }
}
