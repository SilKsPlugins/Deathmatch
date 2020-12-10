using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Deathmatch.Core.Loadouts
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class LoadoutSelector : ILoadoutSelector
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IUserDataStore _userDataStore;
        private readonly ILogger<LoadoutSelector> _logger;

        private readonly Dictionary<IGamePlayer, List<LoadoutSelection>> _loadoutSelections;

        public LoadoutSelector(ILoadoutManager loadoutManager,
            IUserDataStore userDataStore,
            IGamePlayerManager playerManager,
            IEventBus eventBus,
            IRuntime runtime,
            ILogger<LoadoutSelector> logger)
        {
            _loadoutManager = loadoutManager;
            _userDataStore = userDataStore;
            _logger = logger;

            _loadoutSelections = new Dictionary<IGamePlayer, List<LoadoutSelection>>();

            AsyncHelper.RunSync(async () =>
            {
                foreach (var player in playerManager.GetPlayers())
                {
                    await LoadPlayer(player);
                }
            });

            eventBus.Subscribe(runtime, (EventCallback<IGamePlayerConnectedEvent>)OnGamePlayerConnected);
            eventBus.Subscribe(runtime, (EventCallback<IGamePlayerDisconnectedEvent>)OnGamePlayerDisconnected);
        }

        public ILoadout GetLoadout(IGamePlayer player, string category)
        {
            var loadoutCategory = _loadoutManager.GetCategory(category);

            if (loadoutCategory == null) return null;

            var loadoutTitle = _loadoutSelections[player]
                .FirstOrDefault(x => x.GameMode.Equals(category, StringComparison.OrdinalIgnoreCase))?.Loadout;

            return loadoutTitle == null ? null : loadoutCategory.GetLoadout(loadoutTitle);
        }

        public async Task SetLoadout(IGamePlayer player, string category, string loadout)
        {
            var selections = _loadoutSelections[player];

            var selection =
                selections.FirstOrDefault(x => x.GameMode.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (selection == null)
            {
                selection = new LoadoutSelection()
                {
                    GameMode = category,
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
            var selections =
                await _userDataStore.GetUserDataAsync<List<object>>(player.User.Id, player.User.Type,
                    LoadoutSelectionsKey);

            if (selections != null)
            {
                foreach (var selection in selections)
                {
                    var type = selection.GetType();

                    _logger.LogDebug(type.Name + ": " + selection);
                }
            }

            _loadoutSelections.Add(player,
                selections?.OfType<LoadoutSelection>().ToList() ?? new List<LoadoutSelection>());
        }

        private async Task SavePlayer(IGamePlayer player)
        {
            await _userDataStore.SetUserDataAsync(player.User.Id, player.User.Type, LoadoutSelectionsKey,
                _loadoutSelections[player]);

            _loadoutSelections.Remove(player);
        }

        private Task OnGamePlayerConnected(IServiceProvider serviceProvider, object sender,
            IGamePlayerConnectedEvent @event) => LoadPlayer(@event.Player);

        private Task OnGamePlayerDisconnected(IServiceProvider serviceProvider, object sender,
            IGamePlayerDisconnectedEvent @event) => SavePlayer(@event.Player);
    }
}
