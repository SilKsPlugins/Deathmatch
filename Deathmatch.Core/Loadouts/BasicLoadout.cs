using Deathmatch.Core.Items;
using System;

namespace Deathmatch.Core.Loadouts
{
    [Serializable]
    public class BasicLoadout : LoadoutBase<Item>
    {
        public BasicLoadout()
        {
        }

        public BasicLoadout(string title, string? permission) : base(title, permission)
        {
        }

        protected override Item CreateItem(ushort id, byte amount, byte quality, byte[] state)
        {
            return new(id, amount, quality, state);
        }
    }
}
