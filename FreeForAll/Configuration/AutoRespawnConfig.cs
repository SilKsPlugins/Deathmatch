using System;

namespace FreeForAll.Configuration
{
    [Serializable]
    public class AutoRespawnConfig
    {
        public bool Enabled { get; set; } = true;

        public float Delay { get; set; } = 0;
    }
}
