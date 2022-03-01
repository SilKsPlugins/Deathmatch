using System;

namespace TeamDeathmatch.Configuration
{
    [Serializable]
    public class TeamDeathmatchConfiguration
    {
        public AutoRespawnConfig AutoRespawn { get; set; } = new();

        public bool FriendlyFire { get; set; } = false;

        public int KillThreshold { get; set; } = 30;

        public float MaxDuration { get; set; } = 600;

        public float GracePeriod { get; set; } = 2;

        public GameRewardsConfig Rewards { get; set; } = new();
    }
}
