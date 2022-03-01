using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.Core.Players.Extensions;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Players.Life.Events;
using SilK.Unturned.Extras.Configuration;
using System.Threading.Tasks;
using TeamDeathmatch.Configuration;

namespace TeamDeathmatch.Matches
{
    public class MatchEventListener : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IMatchExecutor _matchExecutor;
        private readonly IConfigurationParser<TeamDeathmatchConfiguration> _configuration;

        public MatchEventListener(IMatchExecutor matchExecutor,
            IConfigurationParser<TeamDeathmatchConfiguration> configuration)
        {
            _matchExecutor = matchExecutor;
            _configuration = configuration;
        }

        private bool IsInActiveMatch(UnturnedPlayer player)
        {
            var match = _matchExecutor.CurrentMatch;

            if (match == null || match.Status != MatchStatus.InProgress)
            {
                return false;
            }

            if (match is not MatchTDM)
            {
                return false;
            }

            var matchPlayer = match.GetPlayer(player);

            return matchPlayer != null;
        }

        public async Task HandleEventAsync(object? sender, UnturnedPlayerDeathEvent @event)
        {
            if (!IsInActiveMatch(@event.Player))
            {
                return;
            }

            if (!_configuration.Instance.AutoRespawn.Enabled)
            {
                return;
            }

            var delayConfig = _configuration.Instance.AutoRespawn.Delay;

            async UniTask AutoRespawnPlayer(UnturnedPlayer player, double delay)
            {
                if (delay > 0)
                {
                    await UniTask.Delay((int)(delay * 1000));
                }

                await UniTask.SwitchToMainThread();

                if (IsInActiveMatch(player) && !player.IsAlive)
                {
                    player.ForceRespawn();
                }
            }

            if (delayConfig > 0)
            {
                UniTask.RunOnThreadPool(() => AutoRespawnPlayer(@event.Player, delayConfig)).Forget();
            }
            else
            {
                await AutoRespawnPlayer(@event.Player, 0);
            }
        }
    }
}
