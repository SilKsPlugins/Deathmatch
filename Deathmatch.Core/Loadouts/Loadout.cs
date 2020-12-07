using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Item = Deathmatch.Core.Items.Item;

namespace Deathmatch.Core.Loadouts
{
    [Serializable]
    public class Loadout : ILoadout
    {
        public string Title { get; set; }

        public string Permission { get; set; }

        public List<Item> Items { get; set; }

        public Loadout()
        {
            Title = "";
            Permission = "";
            Items = new List<Item>();
        }

        public Loadout(string title, string permission, List<Item> items)
        {
            Title = title;
            Permission = permission;
            Items = items ?? new List<Item>();
        }

        public string GetPermissionWithoutComponent()
        {
            var index = Permission.IndexOf(':');

            return index < 0 ? Permission : Permission.Substring(index + 1);
        }

        public void GiveToPlayer(IGamePlayer player)
        {
            if (player.IsDead) return;

            player.ClearInventory();
            player.ClearClothing();

            if (Items == null) return;

            foreach (var item in Items)
            {
                item.GiveToPlayer(player);
            }
        }

        public static Loadout FromPlayer(IGamePlayer player)
        {
            var loadout = new Loadout();

            var c = player.Clothing;

            if (c.backpack != 0) loadout.Items.Add(new Item(c.backpack, 1, c.backpackQuality, c.backpackState));
            if (c.glasses != 0) loadout.Items.Add(new Item(c.glasses, 1, c.glassesQuality, c.glassesState));
            if (c.hat != 0) loadout.Items.Add(new Item(c.hat, 1, c.hatQuality, c.hatState));
            if (c.mask != 0) loadout.Items.Add(new Item(c.mask, 1, c.maskQuality, c.maskState));
            if (c.pants != 0) loadout.Items.Add(new Item(c.pants, 1, c.pantsQuality, c.pantsState));
            if (c.shirt != 0) loadout.Items.Add(new Item(c.shirt, 1, c.shirtQuality, c.shirtState));
            if (c.vest != 0) loadout.Items.Add(new Item(c.vest, 1, c.vestQuality, c.vestState));

            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                var count = player.Inventory.getItemCount(page);

                for (byte i = 0; i < count; i++)
                {
                    var jar = player.Inventory.getItem(page, i);

                    if (jar?.item == null) continue;

                    loadout.Items.Add(Item.FromUnturnedItem(jar.item));
                }
            }

            return loadout;
        }
    }
}
