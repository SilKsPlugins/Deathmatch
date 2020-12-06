using Deathmatch.API.Players;
using OpenMod.Unturned.Players.Clothing.Events;
using System.Threading.Tasks;

namespace Deathmatch.Addons.Addons
{
    public class FixedClothing : IAddon,
        IAddonEventListener<UnturnedPlayerClothingUnequippingEvent>
    {
        private readonly IGamePlayerManager _playerManager;

        public FixedClothing(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public string Title => "FixedClothing";

        public void Load()
        {
        }

        public void Unload()
        {
        }

        public Task HandleEventAsync(object sender, UnturnedPlayerClothingUnequippingEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (player.IsInActiveMatch())
                @event.IsCancelled = true;

            return Task.CompletedTask;
        }
    }
}
