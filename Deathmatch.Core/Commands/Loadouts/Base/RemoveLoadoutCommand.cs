using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using SilK.Unturned.Extras.Localization;
using System;

namespace Deathmatch.Core.Commands.Loadouts.Base
{
    public abstract class RemoveLoadoutCommand<TLoadoutCategory, TLoadout, TItem> : UnturnedCommand
        where TLoadoutCategory : LoadoutCategoryBase<TLoadout, TItem>
        where TLoadout : LoadoutBase<TItem>
        where TItem : Item
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IStringLocalizer _stringLocalizer;

        protected RemoveLoadoutCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _loadoutManager = serviceProvider.GetRequiredService<ILoadoutManager>();
            _stringLocalizer = serviceProvider.GetRequiredService<IStringLocalizerAccessor<DeathmatchPlugin>>();
        }

        protected override async UniTask OnExecuteAsync()
        {
            var loadoutTitle = await Context.Parameters.GetAsync<string>(0);

            var category = _loadoutManager.GetCategory<TLoadoutCategory>();

            if (category == null)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);
            }

            var loadout = category.GetLoadout(loadoutTitle);

            if (loadout == null)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_loadout"]);
            }

            category.RemoveLoadout(loadout);

            await category.Save();

            await PrintAsync(_stringLocalizer["commands:delete_loadout:success",
                new { GameMode = category.Title, Loadout = loadout.Title }]);
        }
    }
}
