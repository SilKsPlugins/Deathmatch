using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Prioritization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Core.Commands.Loadouts
{
    [Command("giveloadout", Priority = Priority.Normal)]
    [CommandAlias("givel")]
    [CommandAlias("gloadout")]
    [CommandAlias("gl")]
    [CommandSyntax("<game mode> <loadout>")]
    [CommandDescription("Give yourself the specified loadout.")]
    [CommandActor(typeof(UnturnedUser))]
    public class GiveLoadout : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly ILoadoutManager _loadoutManager;
        private readonly IStringLocalizer _stringLocalizer;

        public GiveLoadout(IGamePlayerManager playerManager,
            ILoadoutManager loadoutManager,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _loadoutManager = loadoutManager;
            _stringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            var gameMode = await Context.Parameters.GetAsync<string>(0);
            var loadoutTitle = await Context.Parameters.GetAsync<string>(1);

            var category = _loadoutManager.GetCategory(gameMode);

            if (category == null)
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);

            var loadout = category.GetLoadout(loadoutTitle);

            if (loadout == null)
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_loadout"]);

            await UniTask.SwitchToMainThread();

            loadout.GiveToPlayer(player);

            await PrintAsync(_stringLocalizer["commands:give_loadout:success",
                new {GameMode = category.Title, Loadout = loadout.Title}]);
        }
    }
}
