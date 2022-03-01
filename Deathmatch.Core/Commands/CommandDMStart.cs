using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Registrations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;

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
            IMatchRegistration? registration = null;

            if (Context.Parameters.Length > 0)
            {
                var title = await Context.Parameters.GetAsync<string>(0, null) ?? string.Empty;

                registration = (string.IsNullOrEmpty(title) ? null : _matchManager.GetMatchRegistration(title))
                               ?? throw new UserFriendlyException(_stringLocalizer["commands:dmstart:not_found"]);
            }

            if (!await _matchExecutor.StartMatch(registration))
            {
                throw new UserFriendlyException(_stringLocalizer["commands:dmstart:match_running"]);
            }

            registration = _matchExecutor.CurrentMatch?.Registration ?? throw new Exception(
                $"Match started but {nameof(IMatchExecutor)}.{nameof(IMatchExecutor.CurrentMatch)} or registration is null");

            await PrintAsync(_stringLocalizer["commands:dmstart:success", new {registration.Title}]);
        }
    }
}
