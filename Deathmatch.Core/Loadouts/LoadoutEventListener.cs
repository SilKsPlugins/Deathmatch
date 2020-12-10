using Deathmatch.API.Loadouts;
using Deathmatch.API.Players.Events;
using OpenMod.API.Eventing;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public class LoadoutEventListener : IEventListener<IGamePlayerConnectedEvent>, IEventListener<IGamePlayerDisconnectedEvent>
    {
        private readonly ILoadoutSelector _loadoutSelector;

        public LoadoutEventListener(ILoadoutSelector loadoutSelector)
        {
            _loadoutSelector = loadoutSelector;
        }

        public async Task HandleEventAsync(object sender, IGamePlayerConnectedEvent @event)
        {
            if (_loadoutSelector is LoadoutSelector selector)
            {
                await selector.LoadPlayer(@event.Player);
            }
        }

        public async Task HandleEventAsync(object sender, IGamePlayerDisconnectedEvent @event)
        {
            if (_loadoutSelector is LoadoutSelector selector)
            {
                await selector.SavePlayer(@event.Player);
            }
        }
    }
}
