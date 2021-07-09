using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Loadouts;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.API.Prioritization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deathmatch.Core.Commands.Loadouts
{
    [Command("loadouts", Priority = Priority.Normal)]
    [CommandSyntax("[game mode]")]
    [CommandDescription("View which loadouts you have unlocked.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandLoadouts : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly ILoadoutManager _loadoutManager;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IPermissionChecker _permissionChecker;

        public CommandLoadouts(IGamePlayerManager playerManager,
            ILoadoutManager loadoutManager,
            IStringLocalizer stringLocalizer,
            IPermissionChecker permissionChecker,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _loadoutManager = loadoutManager;
            _stringLocalizer = stringLocalizer;
            _permissionChecker = permissionChecker;
        }

        private async Task<List<string>> GetUnlockedLoadoutTitles(IPermissionActor actor, ILoadoutCategory category)
        {
            var loadouts = new List<string>();

            foreach (var loadout in category.GetLoadouts())
            {
                if (loadout.Permission == null ||
                    await _permissionChecker.CheckPermissionAsync(actor, loadout.Permission) ==
                    PermissionGrantResult.Grant)
                {
                    loadouts.Add(loadout.Title);
                }
            }

            return loadouts;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            var gameMode = Context.Parameters.Length == 0 ? null : await Context.Parameters.GetAsync<string>(0);

            if (gameMode == null)
            {
                var categories = _loadoutManager.GetCategories();

                var hasLoadouts = false;

                foreach (var category in categories)
                {
                    var loadouts = await GetUnlockedLoadoutTitles(player.User, category);

                    if (loadouts.Count == 0) continue;

                    hasLoadouts = true;

                    await PrintAsync(_stringLocalizer["commands:loadouts:success",
                        new { GameMode = category.Title, Loadouts = loadouts }]);
                }

                if (!hasLoadouts)
                {
                    await PrintAsync(_stringLocalizer["commands:loadouts:none"]);
                }
            }
            else
            {
                var category = _loadoutManager.GetCategory(gameMode, false);

                if (category == null)
                {
                    await PrintAsync(_stringLocalizer["commands:loadout:no_gamemode"]);
                }
                else
                {
                    var loadouts = await GetUnlockedLoadoutTitles(player.User, category);

                    if (loadouts.Count == 0)
                    {
                        await PrintAsync(_stringLocalizer["commands:loadouts:none_gamemode",
                            new { GameMode = category.Title }]);
                    }
                    else
                    {
                        await PrintAsync(_stringLocalizer["commands:loadouts:success",
                            new { GameMode = category.Title, Loadouts = loadouts }]);
                    }
                }
            }
        }
    }
}
