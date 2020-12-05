using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using OpenMod.Core.Helpers;
using System.Threading.Tasks;

namespace Deathmatch.Addons.Addons
{
    public class AutoJoinAddon : IAddon,
        IAddonEventListener<IGamePlayerConnectedEvent>
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly IMatchExecutor _matchExecutor;

        public AutoJoinAddon(IGamePlayerManager playerManager, IMatchExecutor matchExecutor)
        {
            _playerManager = playerManager;
            _matchExecutor = matchExecutor;
        }

        public string Title => "AutoJoin";

        public void Load()
        {
            AsyncHelper.RunSync(async () =>
            {
                foreach (var player in _playerManager.GetPlayers())
                {
                    await _matchExecutor.AddParticipant(player);
                }
            });
        }

        public void Unload()
        {
        }

        public async Task HandleEventAsync(object sender, IGamePlayerConnectedEvent @event)
        {
            await _matchExecutor.AddParticipant(@event.Player);
        }
    }
}
