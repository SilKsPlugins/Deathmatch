using Deathmatch.API.Players;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace Deathmatch.Core.Grace
{
    public class GraceEventsListener : IEventListener<UnturnedPlayerDamagingEvent>
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly IGraceManager _graceManager;

        public GraceEventsListener(IGamePlayerManager playerManager,
            IGraceManager graceManager)
        {
            _playerManager = playerManager;
            _graceManager = graceManager;
        }

        public Task HandleEventAsync(object sender, UnturnedPlayerDamagingEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (_graceManager.WithinGracePeriod(player))
                @event.IsCancelled = true;

            return Task.CompletedTask;
        }
    }
}
