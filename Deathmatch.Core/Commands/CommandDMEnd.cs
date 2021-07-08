using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;

namespace Deathmatch.Core.Commands
{
    [Command("dmend")]
    [CommandDescription("Ends the current match.")]
    public class CommandDMEnd : UnturnedCommand
    {
        private readonly IMatchExecutor _matchExecutor;
        private readonly IStringLocalizer _stringLocalizer;

        public CommandDMEnd(IMatchExecutor matchExecutor,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _matchExecutor = matchExecutor;
            _stringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (_matchExecutor.CurrentMatch != null && _matchExecutor.CurrentMatch.Status == MatchStatus.InProgress)
            {
                await PrintAsync(_stringLocalizer["commands:dmend:success"]);
            }
            else
            {
                await PrintAsync(_stringLocalizer["commands:dmend:failure"]);
            }
        }
    }
}
