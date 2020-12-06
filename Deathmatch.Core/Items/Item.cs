using Deathmatch.API.Players;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Items
{
    [Serializable]
    public class Item
    {
        public string Id { get; set; }

        public byte Amount { get; set; }

        public byte Quality { get; set; }

        public string State { get; set; }

        public Item()
        {
            Id = "";
            Amount = 1;
            Quality = byte.MaxValue;
            State = null;
        }

        public Item(ushort id, byte amount, byte quality, byte[] state)
        {
            Id = id.ToString();
            Amount = amount;
            Quality = quality;
            SetState(state);
        }

        public void SetState(byte[] state)
        {
            State = state == null ? null : string.Join(",", state);
        }

        private ItemAsset _cachedAsset;

        public ItemAsset GetAsset()
        {
            if (_cachedAsset != null) return _cachedAsset;

            if (string.IsNullOrWhiteSpace(Id)) return null;

            if (ushort.TryParse(Id, out ushort parsed))
            {
                _cachedAsset = Assets.find(EAssetType.ITEM, parsed) as ItemAsset;

                if (_cachedAsset != null) return _cachedAsset;
            }

            List<ItemAsset> possibilities = new List<ItemAsset>();

            string lowered = Id.ToLower();

            foreach (ItemAsset asset in Assets.find(EAssetType.ITEM).OfType<ItemAsset>())
            {
                if (string.IsNullOrWhiteSpace(asset.itemName)) continue;

                if (asset.itemName.ToLower().Contains(lowered))
                {
                    possibilities.Add(asset);
                }
            }

            int LevenshteinDistance(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];
                if (n == 0)
                {
                    return m;
                }
                if (m == 0)
                {
                    return n;
                }
                for (int i = 0; i <= n; d[i, 0] = i++)
                    ;
                for (int j = 0; j <= m; d[0, j] = j++)
                    ;
                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                return d[n, m];
            }

            _cachedAsset = possibilities
                .OrderBy(x => LevenshteinDistance(x.itemName.ToLower(), lowered))
                .FirstOrDefault();

            return _cachedAsset;
        }

        public virtual bool GiveToPlayer(IGamePlayer player)
        {
            if (player.IsDead) return false;

            if (GetAsset() == null) return false;

            for (int i = 0; i < Amount; i++)
            {
                var item = new SDG.Unturned.Item(
                    GetAsset().id,
                    GetAsset().amount,
                    100,
                    State == null
                        ? GetAsset().getState(EItemOrigin.ADMIN)
                        : State.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToArray());

                player.Inventory.forceAddItem(item, true);
            }

            return true;
        }

        public static Item FromUnturnedItem(SDG.Unturned.Item unturnedItem) => new Item(unturnedItem.id,
            unturnedItem.amount, unturnedItem.quality, unturnedItem.state);
    }
}
