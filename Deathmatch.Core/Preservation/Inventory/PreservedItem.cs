using SDG.Unturned;

namespace Deathmatch.Core.Preservation.Inventory
{
    public class PreservedItem
    {
        private readonly ushort _id;
        private readonly byte _amount;
        private readonly byte _quality;
        private readonly byte[] _state;

        public PreservedItem(Item item)
        {
            _id = item.id;
            _amount = item.amount;
            _quality = item.quality;
            _state = item.state;
        }

        public Item Restore() => new Item(_id, _amount, _quality, _state);
    }
}
