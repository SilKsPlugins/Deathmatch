using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Loadouts;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Prioritization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Core.Commands.Loadouts
{
    [Command("loadout", Priority = Priority.Normal)]
    [CommandSyntax("<game mode> <loadout>")]
    [CommandDescription("Select your loadout for the given game mode.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandLoadout : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IPermissionChecker _permissionChecker;

        public CommandLoadout(IGamePlayerManager playerManager,
            ILoadoutManager loadoutManager,
            ILoadoutSelector loadoutSelector,
            IStringLocalizer stringLocalizer,
            IPermissionChecker permissionChecker,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _loadoutManager = loadoutManager;
            _loadoutSelector = loadoutSelector;
            _stringLocalizer = stringLocalizer;
            _permissionChecker = permissionChecker;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            var gameMode = await Context.Parameters.GetAsync<string>(0);
            var loadoutTitle = await Context.Parameters.GetAsync<string>(1);

            var category = _loadoutManager.GetCategory(gameMode);

            if (category == null)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);
            }

            var loadout = category.GetLoadout(loadoutTitle, false);

            if (loadout == null)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_loadout"]);
            }

            if (loadout.Permission != null &&
                await _permissionChecker.CheckPermissionAsync(Context.Actor, loadout.Permission) !=
                PermissionGrantResult.Grant)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_permission"]);
            }

            await _loadoutSelector.SetLoadout(player, category.Title, loadout.Title);

            await PrintAsync(_stringLocalizer["commands:loadout:success", new { GameMode = category.Title, Loadout = loadout.Title }]);
        }
    }
}
