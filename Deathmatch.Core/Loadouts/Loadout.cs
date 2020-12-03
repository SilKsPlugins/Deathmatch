using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Items;
using System;
using System.Collections.Generic;

namespace Deathmatch.Core.Loadouts
{
    [Serializable]
    public class Loadout : ILoadout
    {
        public string Title { get; set; }

        public List<Item> Items { get; set; }

        public string Permission { get; set; }

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
    }
}
