using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer _stringLocalizer;

        public CommandJoin(IServiceProvider serviceProvider,
            IGamePlayerManager playerManager,
            IMatchExecutor matchExecutor,
            IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _matchExecutor = matchExecutor;
            _stringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            if (await _matchExecutor.AddParticipant(player))
            {
                await player.PrintMessageAsync(_stringLocalizer["commands:join:success"]);
            }
            else
            {

                await player.PrintMessageAsync(_stringLocalizer["commands:join:failure"]);
            }
        }
    }
}