using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using JetBrains.Annotations;
using OpenMod.Unturned.Players.Clothing.Events;
using SilK.Unturned.Extras.Events;

namespace Deathmatch.Addons.Addons
{
    [UsedImplicitly]
    public class FixedClothing : AddonBase,
        IInstanceEventListener<UnturnedPlayerClothingUnequippingEvent>
    {
        public override string Title => "FixedClothing";

        private readonly IGamePlayerManager _playerManager;

        public FixedClothing(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public UniTask HandleEventAsync(object? sender, UnturnedPlayerClothingUnequippingEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (player.IsInActiveMatch())
            {
                @event.IsCancelled = true;
            }

            return UniTask.CompletedTask;
        }
    }
}
