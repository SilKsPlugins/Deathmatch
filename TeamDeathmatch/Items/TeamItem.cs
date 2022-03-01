using Deathmatch.API.Players;
using Deathmatch.Core.Items;
using System;
using TeamDeathmatch.Players;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Items
{
    [Serializable]
    public class TeamItem : Item
    {
        public TeamItem()
        {
        }

        public TeamItem(ushort id, byte amount, byte quality, byte[] state) : base(id, amount, quality, state)
        {
        }

        public Team Team { get; set; }

        public override bool GiveToPlayer(IGamePlayer player)
        {
            if (Team != Team.None && Team != player.GetTeam())
            {
                return false;
            }

            return base.GiveToPlayer(player);
        }
    }
}
