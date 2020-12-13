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
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            await _matchExecutor.AddParticipant(player);
        }
    }
}