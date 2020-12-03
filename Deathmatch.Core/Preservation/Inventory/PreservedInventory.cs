using SDG.Unturned;
using System.Collections.Generic;

namespace Deathmatch.Core.Preservation.Inventory
{
    public class PreservedInventory
    {
        private readonly List<PreservedInventoryPage> _inventoryPages;

        public PreservedInventory(PlayerInventory inventory)
        {
            _inventoryPages = new List<PreservedInventoryPage>();

            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                if (inventory.items[page] == null) continue;

                _inventoryPages.Add(new PreservedInventoryPage(page, inventory.items[page]));
            }
        }
        
        public void Restore(PlayerInventory inventory)
        {
            foreach (var page in _inventoryPages)
            {
                page.Restore(inventory.items[page.Page]);
            }

            inventory.player.equipment.sendSlot(0);
            inventory.player.equipment.sendSlot(1);
        }
    }
}
