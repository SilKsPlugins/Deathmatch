using Deathmatch.API.Players;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Users.Events;
using System.Threading.Tasks;

namespace Deathmatch.Core.Players
{
    [EventListenerLifetime(ServiceLifetime.Singleton)]
    public class GamePlayerEventsListener :
        IEventListener<UnturnedUserConnectedEvent>,
        IEventListener<UnturnedUserDisconnectedEvent>
    {
        private readonly IGamePlayerManager _playerManager;

        public GamePlayerEventsListener(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public Task HandleEventAsync(object sender, UnturnedUserConnectedEvent @event)
        {
            if (_playerManager is GamePlayerManager matchPlayerManager)
            {
                matchPlayerManager.OnUserConnected(@event.User);
            }

            return Task.CompletedTask;
        }

        public Task HandleEventAsync(object sender, UnturnedUserDisconnectedEvent @event)
        {
            if (_playerManager is GamePlayerManager matchPlayerManager)
            {
                matchPlayerManager.OnUserDisconnected(@event.User);
            }

            return Task.CompletedTask;
        }
    }
}
