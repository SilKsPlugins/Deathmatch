using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using SDG.Unturned;
using System;
using System.Linq;

namespace Deathmatch.Core.Items
{
    [Serializable]
    public class Item
    {
        public string Id { get; set; }

        public byte Amount { get; set; }

        public byte Quality { get; set; }

        public string? State { get; set; }

        public Item()
        {
            Id = "";
            Amount = 1;
            Quality = 100;
            State = null;
        }

        public Item(ushort id, byte amount, byte quality, byte[] state)
        {
            Id = id.ToString();
            Amount = amount;
            Quality = quality;
            SetState(state);
        }

        public void SetState(byte[]? state)
        {
            State = state == null ? null : string.Join(",", state);
        }

        private ItemAsset? _cachedAsset;

        public ItemAsset? GetAsset()
        {
            if (_cachedAsset != null)
            {
                return _cachedAsset;
            }

            if (string.IsNullOrWhiteSpace(Id))
            {
                return null;
            }

            if (ushort.TryParse(Id, out var parsed))
            {
                _cachedAsset = Assets.find(EAssetType.ITEM, parsed) as ItemAsset;
            }

            return _cachedAsset ??= Assets.find(EAssetType.ITEM).OfType<ItemAsset>().Where(x => x != null).FindBestMatch(x => x.itemName, Id);
        }

        public virtual bool GiveToPlayer(IGamePlayer player)
        {
            if (player.IsDead)
            {
                return false;
            }

            var asset = GetAsset();

            if (asset == null)
            {
                return false;
            }

            var item = new SDG.Unturned.Item(
                asset.id,
                Amount,
                Quality,
                string.IsNullOrWhiteSpace(State)
                    ? asset.getState(EItemOrigin.ADMIN)
                    : State!.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToArray());

            player.Inventory.forceAddItem(item, true);

            return true;
        }

        public static Item FromUnturnedItem(SDG.Unturned.Item unturnedItem) => new(unturnedItem.id, unturnedItem.amount,
            unturnedItem.quality, unturnedItem.state);
    }
}
