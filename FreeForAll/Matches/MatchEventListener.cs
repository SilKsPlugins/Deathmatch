﻿using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace FreeForAll.Matches
{
    public class MatchEventListener : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IMatchExecutor _matchExecutor;
        private readonly IConfiguration _configuration;

        public MatchEventListener(IMatchExecutor matchExecutor,
            IConfiguration configuration)
        {
            _matchExecutor = matchExecutor;
            _configuration = configuration;
        }

        private bool IsInActiveMatch(UnturnedPlayer player)
        {
            var match = _matchExecutor.CurrentMatch;

            if (match == null || !match.IsRunning) return false;

            if (!(match is MatchFFA ffaMatch)) return false;

            var matchPlayer = ffaMatch.GetPlayer(player);

            return matchPlayer != null;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
        {
            if (!IsInActiveMatch(@event.Player)) return;

            if (!_configuration.GetValue("AutoRespawn:Enabled", false)) return;

            var delayConfig = _configuration.GetValue<double>("AutoRespawn:Delay", 0);

            async UniTask AutoRespawnPlayer(UnturnedPlayer player, double delay)
            {
                await UniTask.SwitchToMainThread();

                if (delay > 0)
                {
                    await UniTask.Delay((int)(delay * 1000));
                }

                if (IsInActiveMatch(player) && !player.IsAlive)
                {
                    player.Player.life.askRespawn(player.SteamId, false);
                }
            }

            if (delayConfig > 0)
            {
                AutoRespawnPlayer(@event.Player, delayConfig).Forget();
            }
            else
            {
                await AutoRespawnPlayer(@event.Player, 0);
            }
        }
    }
}
