using Deathmatch.API.Players;
using System;

namespace Deathmatch.Core.Items
{
    [Serializable]
    public class ChanceItem : Item
    {
        private static readonly Random Rng = new Random();

        public float Chance { get; set; } = 1;

        public override bool GiveToPlayer(IGamePlayer player)
        {
            if (Rng.NextDouble() > Chance) return false;

            return base.GiveToPlayer(player);
        }
    }
}
