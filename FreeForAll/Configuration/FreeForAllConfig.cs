using System;

namespace FreeForAll.Configuration
{
    [Serializable]
    public class FreeForAllConfig
    {
        public AutoRespawnConfig AutoRespawn { get; set; } = new();

        public int KillThreshold { get; set; } = 30;

        public float MaxDuration { get; set; } = 600;

        public float GracePeriod { get; set; } = 2;

        public GameRewardsConfig Rewards { get; set; } = new();
    }
}
