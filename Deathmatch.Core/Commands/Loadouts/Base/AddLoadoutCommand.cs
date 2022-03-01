using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Items;
using Deathmatch.Core.Loadouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SilK.Unturned.Extras.Localization;
using System;

namespace Deathmatch.Core.Commands.Loadouts.Base
{
    public abstract class AddLoadoutCommand<TLoadoutCategory, TLoadout, TItem> : UnturnedCommand
        where TLoadoutCategory : LoadoutCategoryBase<TLoadout, TItem>
        where TLoadout : LoadoutBase<TItem>
        where TItem : Item
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly ILoadoutManager _loadoutManager;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IPermissionRegistry _permissionRegistry;

        protected AddLoadoutCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = serviceProvider.GetRequiredService<IGamePlayerManager>();
            _loadoutManager = serviceProvider.GetRequiredService<ILoadoutManager>();
            _stringLocalizer = serviceProvider.GetRequiredService<IStringLocalizerAccessor<DeathmatchPlugin>>();
            _permissionRegistry = serviceProvider.GetRequiredService<IPermissionRegistry>();
        }

        protected abstract TLoadout CreateLoadout(string title, string permission);

        protected override async UniTask OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();

            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);
            
            var loadoutTitle = await Context.Parameters.GetAsync<string>(0);

            var category = _loadoutManager.GetCategory<TLoadoutCategory>();

            if (category == null)
            {
                throw new UserFriendlyException(_stringLocalizer["commands:loadout:no_gamemode"]);
            }

            var oldLoadout = category.GetLoadout(loadoutTitle);

            var title = loadoutTitle.ToLower();
            var permission = "loadouts." + title;

            var newLoadout = CreateLoadout(title, permission);

            await newLoadout.LoadItemsFromPlayer(player);

            if (oldLoadout != null)
            {
                category.RemoveLoadout(oldLoadout);
            }

            _permissionRegistry.RegisterPermission(category.Component, permission);

            category.AddLoadout(newLoadout);

            await category.Save();

            await PrintAsync(_stringLocalizer[
                "commands:create_loadout:success" + (oldLoadout == null ? "" : "_overwrite"),
                new { Loadout = newLoadout.Title, GameMode = category.Title }]);
        }
    }
}
