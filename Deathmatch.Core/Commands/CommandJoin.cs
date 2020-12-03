using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Core.Commands
{
    [Command("join")]
    [CommandSyntax("")]
    [CommandDescription("Joins the current match/match pool.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandJoin : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly IMatchExecutor _matchExecutor;

        public CommandJoin(IGamePlayerManager playerManager,
            IMatchExecutor matchExecutor,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _matchExecutor = matchExecutor;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            if (user == null) return;

            var player = _playerManager.GetPlayer(x => x.SteamId == user.SteamId);

            if (player == null) return;

            await _matchExecutor.AddParticipant(player);
        }
    }
}