using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Deathmatch.Hub.Commands
{
    [Command("hub")]
    [CommandDescription("Teleports the player to the hub.")]
    [CommandSyntax("")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandHub : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly DeathmatchHubPlugin _plugin;

        public CommandHub(IGamePlayerManager playerManager,
            IStringLocalizer stringLocalizer,
            DeathmatchHubPlugin plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _stringLocalizer = stringLocalizer;
            _plugin = plugin;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer(Context.Actor as UnturnedUser);

            if (player.CurrentMatch != null)
                throw new UserFriendlyException(_stringLocalizer["commands:hub:in_match"]);

            if (_plugin.Hub == null)
                throw new UserFriendlyException(_stringLocalizer["commands:hub:not_set"]);

            await UniTask.SwitchToMainThread();

            _plugin.Hub.TeleportPlayer(player.Player);

            await PrintAsync(_stringLocalizer["commands:hub:success"]);
        }
    }
}
