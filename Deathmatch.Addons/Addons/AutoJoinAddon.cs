using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using JetBrains.Annotations;
using SilK.Unturned.Extras.Events;

namespace Deathmatch.Addons.Addons
{
    [UsedImplicitly]
    public class AutoJoinAddon : AddonBase,
        IInstanceEventListener<IGamePlayerConnectedEvent>
    {
        public override string Title => "AutoJoin";

        private readonly IGamePlayerManager _playerManager;
        private readonly IMatchExecutor _matchExecutor;

        public AutoJoinAddon(IGamePlayerManager playerManager,
            IMatchExecutor matchExecutor)
        {
            _playerManager = playerManager;
            _matchExecutor = matchExecutor;
        }

        protected override async UniTask OnLoadAsync()
        {
            foreach (var player in _playerManager.GetPlayers())
            {
                await _matchExecutor.AddParticipant(player);
            }
        }

        public async UniTask HandleEventAsync(object? sender, IGamePlayerConnectedEvent @event)
        {
            await _matchExecutor.AddParticipant(@event.Player);
        }
    }
}
