using Deathmatch.Core.Loadouts;
using System;
using TeamDeathmatch.Items;

namespace TeamDeathmatch.Loadouts
{
    [Serializable]
    public class TeamLoadout : LoadoutBase<TeamItem>
    {
        public TeamLoadout()
        {
        }

        public TeamLoadout(string title, string? permission) : base(title, permission)
        {
        }

        protected override TeamItem CreateItem(ushort id, byte amount, byte quality, byte[] state)
        {
            return new(id, amount, quality, state);
        }
    }
}
