using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class LoadoutSelector : ILoadoutSelector
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IUserDataStore _userDataStore;

        private readonly Dictionary<IGamePlayer, List<LoadoutSelection>> _loadoutSelections;

        public LoadoutSelector(ILoadoutManager loadoutManager,
            IUserDataStore userDataStore,
            IEventBus eventBus,
            IRuntime runtime)
        {
            _loadoutManager = loadoutManager;
            _userDataStore = userDataStore;

            _loadoutSelections = new Dictionary<IGamePlayer, List<LoadoutSelection>>();

            eventBus.Subscribe(runtime, (EventCallback<GamePlayerConnectedEvent>)OnGamePlayerConnected);
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

            var selection = selections.FirstOrDefault(x => x.GameMode.Equals(category, StringComparison.OrdinalIgnoreCase));

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

        private async Task OnGamePlayerConnected(IServiceProvider serviceProvider, object sender, GamePlayerConnectedEvent @event)
        {
            var player = @event.Player;
            
            var selections =
                await _userDataStore.GetUserDataAsync<List<LoadoutSelection>>(player.User.Id, player.User.Type,
                    LoadoutSelectionsKey);

            selections ??= new List<LoadoutSelection>();

            _loadoutSelections.Add(player, selections);
        }

        private async Task OnGamePlayerDisconnected(IServiceProvider serviceProvider, object sender, IGamePlayerDisconnectedEvent @event)
        {
            var player = @event.Player;

            await _userDataStore.SetUserDataAsync(player.User.Id, player.User.Type, LoadoutSelectionsKey,
                _loadoutSelections[player]);

            _loadoutSelections.Remove(player);
        }
    }
}
