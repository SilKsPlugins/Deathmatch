using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Items;
using System.Collections.Generic;

namespace Deathmatch.Core.Loadouts
{
    public abstract class LoadoutBase : ILoadout
    {
        public string Title { get; set; }

        public string? Permission { get; set; }

        protected LoadoutBase(string title, string? permission)
        {
            Title = title;
            Permission = permission;
        }

        public abstract IReadOnlyCollection<Item> GetItems();

        public string? GetPermissionWithoutComponent()
        {
            if (Permission == null)
            {
                return null;
            }

            var index = Permission.IndexOf(':');

            return index < 0 ? Permission : Permission.Substring(index + 1);
        }

        public void GiveToPlayer(IGamePlayer player)
        {
            if (player.IsDead)
            {
                return;
            }

            player.ClearInventory();
            player.ClearClothing();

            foreach (var item in GetItems())
            {
                item.GiveToPlayer(player);
            }
        }
    }
}
