using Deathmatch.API.Players;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Users.Events;
using System.Threading.Tasks;

namespace Deathmatch.Core.Players
{
    public class GamePlayerEventListener : IEventListener<UnturnedUserConnectedEvent>, IEventListener<UnturnedUserDisconnectedEvent>
    {
        private readonly IGamePlayerManager _playerManager;

        public GamePlayerEventListener(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public async Task HandleEventAsync(object sender, UnturnedUserConnectedEvent @event)
        {
            if (_playerManager is GamePlayerManager manager)
            {
                await manager.AddUser(@event.User);
            }
        }

        public async Task HandleEventAsync(object sender, UnturnedUserDisconnectedEvent @event)
        {
            if (_playerManager is GamePlayerManager manager)
            {
                await manager.RemoveUser(@event.User);
            }
        }
    }
}
