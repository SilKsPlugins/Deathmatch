using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace Deathmatch.Addons.Addons
{
    public class MaxSkillsAddon : IAddon,
        IAddonEventListener<UnturnedPlayerRevivedEvent>,
        IAddonEventListener<IGamePlayerJoinedMatchEvent>
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

        public Task HandleEventAsync(object sender, UnturnedPlayerRevivedEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (player.IsInActiveMatch())
            {
                player.MaxSkills();
            }

            return Task.CompletedTask;
        }

        public Task HandleEventAsync(object sender, IGamePlayerJoinedMatchEvent @event)
        {
            @event.Player.MaxSkills();

            return Task.CompletedTask;
        }
    }
}
