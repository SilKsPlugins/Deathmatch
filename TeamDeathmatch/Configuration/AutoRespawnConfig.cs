using System;

namespace TeamDeathmatch.Configuration
{
    [Serializable]
    public class AutoRespawnConfig
    {
        public bool Enabled { get; set; } = true;

        public float Delay { get; set; } = 0;
    }
}
