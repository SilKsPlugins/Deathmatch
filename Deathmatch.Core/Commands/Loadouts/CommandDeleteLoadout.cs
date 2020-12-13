using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Prioritization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Core.Commands.Loadouts
{
    [Command("deleteloadout", Priority = Priority.Normal)]
    [CommandSyntax("<game mode> <loadout>")]
    [CommandDescription("Delete the specified loadout.")]
    [CommandActor(typeof(UnturnedUser))]
    public class DeleteLoadout : UnturnedCommand
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IStringLocalizer _stringLocalizer;

        public DeleteLoadout(ILoadoutManager loadoutManager,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _loadoutManager = loadoutManager;
            _stringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var gameMode = await Context.Parameters.GetAsync<string>(0);
            var loadoutTitle = await Context.Parameters.GetAsync<string>(1);

            var category = _loadoutManager.GetCategory(gameMode);

            if (category == null)
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);

            var loadout = category.GetLoadout(loadoutTitle);

            if (loadout == null)
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_loadout"]);

            category.RemoveLoadout(loadout);

            await category.SaveLoadouts();

            await PrintAsync(_stringLocalizer["commands:delete_loadout:success",
                new { GameMode = category.Title, Loadout = loadout.Title }]);
        }
    }
}
