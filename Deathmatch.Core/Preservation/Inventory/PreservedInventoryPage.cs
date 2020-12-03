using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Preservation.Inventory
{
    public class PreservedInventoryPage
    {
        public readonly byte Page;

        private readonly List<PreservedItemJar> _itemJars;

        public PreservedInventoryPage(byte page, SDG.Unturned.Items inventoryPage)
        {
            Page = page;
            _itemJars = new List<PreservedItemJar>();

            foreach (ItemJar jar in inventoryPage.items)
            {
                _itemJars.Add(new PreservedItemJar(jar));
            }
        }

        public void Restore(SDG.Unturned.Items items)
        {
            for (int i = items.getItemCount() - 1; i >= 0; i--)
            {
                items.removeItem((byte)i);
            }

            foreach (var jar in _itemJars.Select(x => x.Restore()))
            {
                items.addItem(jar.x, jar.y, jar.rot, jar.item);
            }
        }
    }
}
