using Deathmatch.Core.Items;
using System;

namespace FreeForAll.Configuration
{
    [Serializable]
    public class GameRewardsConfig
    {
        public int MinimumPlayers { get; set; } = 5;

        public ChanceItem[] Winners { get; set; } = new ChanceItem[0];

        public ChanceItem[] Losers { get; set; } = new ChanceItem[0];

        public ChanceItem[] Tied { get; set; } = new ChanceItem[0];

        public ChanceItem[] All { get; set; } = new ChanceItem[0];
    }
}
