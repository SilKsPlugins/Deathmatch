using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Loadouts;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Core.Commands.Loadouts
{
    //[Command("loadout", Priority = Priority.Normal)]
    //[CommandSyntax("<game mode> <loadout>")]
    //[CommandDescription("Select your loadout for the given game mode.")]
    //[CommandActor(typeof(UnturnedUser))]
    public abstract class CLoadout : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutSelector _loadoutSelector;
        private readonly IStringLocalizer _stringLocalizer; 
        private readonly IPermissionChecker _permissionChecker;

        protected CLoadout(IGamePlayerManager playerManager,
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

            var category = _loadoutManager.GetCategory(gameMode) ??
                           throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);

            var loadout = category.GetLoadout(loadoutTitle, false) ??
                          throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_loadout"]);

            if (!await loadout.IsPermitted(player.User))
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_permission"]);
            }

            _loadoutSelector.SetSelectedLoadout(player, category, loadout);

            await PrintAsync(_stringLocalizer["commands:loadout:success", new { GameMode = category.Title, Loadout = loadout.Title }]);
        }
    }
}
