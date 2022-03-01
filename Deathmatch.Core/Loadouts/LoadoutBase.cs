using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Permissions;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Item = Deathmatch.Core.Items.Item;

namespace Deathmatch.Core.Loadouts
{
    [Serializable]
    public abstract class LoadoutBase<TItem> : ILoadout where TItem : Item
    {
        private class LoadoutServices
        {
            // ReSharper disable MemberCanBePrivate.Local
            public readonly IOpenModComponent Component;
            public readonly IPermissionChecker PermissionChecker;
            public readonly IPermissionRegistry PermissionRegistry;
            // ReSharper restore MemberCanBePrivate.Local

            public LoadoutServices(IOpenModComponent component,
                IPermissionChecker permissionChecker,
                IPermissionRegistry permissionRegistry)
            {
                Component = component;
                PermissionChecker = permissionChecker;
                PermissionRegistry = permissionRegistry;
            }

            public static LoadoutServices CreateInstance(IServiceProvider serviceProvider)
            {
                return ActivatorUtilities.CreateInstance<LoadoutServices>(serviceProvider);
            }
        }

        public string Title { get; set; }

        public string? Permission { get; set; }

        public ICollection<TItem> Items { get; set; } = new List<TItem>();

        private LoadoutServices? _services;

        protected LoadoutBase()
        {
            Title = "";
            Permission = null;
        }

        protected LoadoutBase(string title, string? permission)
        {
            Title = title;
            Permission = permission;
        }

        public virtual void ProvideServices(IServiceProvider serviceProvider)
        {
            _services = LoadoutServices.CreateInstance(serviceProvider);

            if (Permission != null)
            {
                _services.PermissionRegistry.RegisterPermission(_services.Component, Permission, $"Grants access to the '{Title}' loadout.");
            }
        }

        protected abstract TItem CreateItem(ushort id, byte amount, byte quality, byte[] state);

        public virtual async UniTask GiveToPlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            if (player.IsDead)
            {
                return;
            }

            player.ClearInventory();
            player.ClearClothing();

            foreach (var item in Items)
            {
                item.GiveToPlayer(player);
            }
        }

        public async UniTask LoadItemsFromPlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            Items.Clear();

            void AddItem(ushort id, byte amount, byte quality, byte[] state)
            {
                if (id == 0)
                {
                    return;
                }

                var copiedState = new byte[state.Length];
                Array.Copy(state, copiedState, state.Length);
                
                var item = CreateItem(id, amount, quality, copiedState);

                Items.Add(item);
            }

            var c = player.Clothing;

            AddItem(c.backpack, 1, c.backpackQuality, c.backpackState);
            AddItem(c.glasses, 1, c.glassesQuality, c.glassesState);
            AddItem(c.hat, 1, c.hatQuality, c.hatState);
            AddItem(c.mask, 1, c.maskQuality, c.maskState);
            AddItem(c.pants, 1, c.pantsQuality, c.pantsState);
            AddItem(c.shirt, 1, c.shirtQuality, c.shirtState);
            AddItem(c.vest, 1, c.vestQuality, c.vestState);

            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                var count = player.Inventory.getItemCount(page);

                for (byte i = 0; i < count; i++)
                {
                    var item = player.Inventory.getItem(page, i)?.item;
                    
                    if (item == null)
                    {
                        continue;
                    }

                    AddItem(item.id, item.amount, item.quality, item.state);
                }
            }
        }

        public async Task<bool> IsPermitted(IPermissionActor actor)
        {
            if (Permission == null)
            {
                return true;
            }

            if (_services == null)
            {
                return false;
            }

            return await _services.PermissionChecker.CheckPermissionAsync(actor, Permission) == PermissionGrantResult.Grant;
        }
    }
}
