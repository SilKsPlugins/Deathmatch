using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Eventing;
using OpenMod.Core.Commands.Events;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Matches
{
    public class MatchEventListener :
        IEventListener<UnturnedUserDisconnectedEvent>,
        IEventListener<CommandExecutingEvent>
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly IMatchExecutor _matchExecutor;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _stringLocalizer;

        public MatchEventListener(IGamePlayerManager playerManager,
            IMatchExecutor matchExecutor,
            IConfiguration configuration,
            IStringLocalizer stringLocalizer)
        {
            _playerManager = playerManager;
            _matchExecutor = matchExecutor;
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
        }

        public Task HandleEventAsync(object? sender, UnturnedUserDisconnectedEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.User);

            if (_matchExecutor.GetParticipants().Contains(player))
            {
                _matchExecutor.RemoveParticipant(player);
            }

            return Task.CompletedTask;
        }

        public Task HandleEventAsync(object? sender, CommandExecutingEvent @event)
        {
            if (@event.Actor is UnturnedUser user)
            {
                var player = _playerManager.GetPlayer(user);

                if (_matchExecutor.CurrentMatch != null && _matchExecutor.CurrentMatch.Status == MatchStatus.InProgress &&
                    _matchExecutor.CurrentMatch.Players.Contains(player))
                {
                    bool IsEqual(string command, ICommandContext context)
                    {
                        if (command.Equals(context.CommandAlias, StringComparison.OrdinalIgnoreCase)) return true;

                        if (context.CommandRegistration == null) return false;
                        if (context.CommandRegistration.Id.Equals(context.CommandAlias,
                            StringComparison.OrdinalIgnoreCase)) return true;
                        if (context.CommandRegistration.Name.Equals(context.CommandAlias,
                            StringComparison.OrdinalIgnoreCase)) return true;

                        if (context.CommandRegistration.Aliases == null) return false;

                        return context.CommandRegistration.Aliases.Any(x =>
                            command.Equals(x, StringComparison.OrdinalIgnoreCase));
                    }

                    if (_configuration.GetValue("DisabledCommands", new string[0])
                        .Any(x => IsEqual(x, @event.CommandContext)))
                    {
                        @event.CommandContext.Exception = new UserFriendlyException(
                            _stringLocalizer["commands:disabled_during_match",
                                new { Command = @event.CommandContext.CommandAlias }]);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
