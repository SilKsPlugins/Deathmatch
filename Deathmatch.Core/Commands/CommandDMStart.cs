using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.Core.Helpers;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;
using Deathmatch.Core.Matches.Extensions;

namespace Deathmatch.Core.Commands
{
    [Command("dmstart")]
    [CommandSyntax("[game mode]")]
    [CommandDescription("Starts a match of either the specified or a random game mode.")]
    public class CommandDMStart : UnturnedCommand
    {
        private readonly IMatchManager _matchManager;
        private readonly IMatchExecutor _matchExecutor;
        private readonly IStringLocalizer _stringLocalizer;

        public CommandDMStart(IMatchManager matchManager,
            IMatchExecutor matchExecutor,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _matchManager = matchManager;
            _matchExecutor = matchExecutor;
            _stringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var title = await Context.Parameters.GetAsync<string>(0, null);

            var registration = string.IsNullOrWhiteSpace(title)
                ? _matchManager.GetMatchRegistrations().RandomElement()
                : _matchManager.GetMatchRegistration(title!);

            if (registration == null)
            {
                await PrintAsync(_stringLocalizer["commands:dmstart:not_found"]);
                return;
            }

            if (await _matchExecutor.StartMatch(registration))
            {
                await PrintAsync(_stringLocalizer["commands:dmstart:success",
                    new { registration.Title }]);
            }
            else
            {
                await PrintAsync(_stringLocalizer["commands:dmstart:failure"]);
            }
        }
    }
}
