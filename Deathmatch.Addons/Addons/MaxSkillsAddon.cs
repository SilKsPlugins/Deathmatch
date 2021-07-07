using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using JetBrains.Annotations;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Events;

namespace Deathmatch.Addons.Addons
{
    [UsedImplicitly]
    public class MaxSkillsAddon : AddonBase,
        IInstanceEventListener<UnturnedPlayerRevivedEvent>,
        IInstanceEventListener<IGamePlayerJoinedMatchEvent>
    {
        public override string Title => "MaxSkills";

        private readonly IGamePlayerManager _playerManager;

        public MaxSkillsAddon(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        protected override UniTask OnLoadAsync()
        {
            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.IsInActiveMatch())
                {
                    player.MaxSkills();
                }
            }

            return UniTask.CompletedTask;
        }

        public UniTask HandleEventAsync(object? sender, UnturnedPlayerRevivedEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (player.IsInActiveMatch())
            {
                player.MaxSkills();
            }

            return UniTask.CompletedTask;
        }

        public UniTask HandleEventAsync(object? sender, IGamePlayerJoinedMatchEvent @event)
        {
            @event.Player.MaxSkills();

            return UniTask.CompletedTask;
        }
    }
}
