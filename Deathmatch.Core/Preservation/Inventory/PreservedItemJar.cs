using SDG.Unturned;

namespace Deathmatch.Core.Preservation.Inventory
{
    public class PreservedItemJar
    {
        private readonly PreservedItem _item;
        private readonly byte _x;
        private readonly byte _y;
        private readonly byte _rotation;

        public PreservedItemJar(ItemJar jar)
        {
            _x = jar.x;
            _y = jar.y;
            _rotation = jar.rot;

            _item = new PreservedItem(jar.item);
        }

        public ItemJar Restore() => new ItemJar(_x, _y, _rotation, _item.Restore());
    }
}
