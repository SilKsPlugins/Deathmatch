using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Events;

namespace Deathmatch.Addons.Addons
{
    public class MaxSkillsAddon : IAddon,
        IInstanceEventListener<UnturnedPlayerRevivedEvent>,
        IInstanceEventListener<IGamePlayerJoinedMatchEvent>
    {
        private readonly IGamePlayerManager _playerManager;

        public MaxSkillsAddon(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public string Title => "MaxSkills";

        public void Load()
        {
            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.IsInActiveMatch())
                {
                    player.MaxSkills();
                }
            }
        }

        public void Unload()
        {
        }

        public UniTask HandleEventAsync(object sender, UnturnedPlayerRevivedEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (player.IsInActiveMatch())
            {
                player.MaxSkills();
            }

            return UniTask.CompletedTask;
        }

        public UniTask HandleEventAsync(object sender, IGamePlayerJoinedMatchEvent @event)
        {
            @event.Player.MaxSkills();

            return UniTask.CompletedTask;
        }
    }
}
