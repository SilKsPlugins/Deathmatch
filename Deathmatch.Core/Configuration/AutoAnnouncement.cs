using System;

namespace Deathmatch.Core.Configuration
{
    [Serializable]
    public class AutoAnnouncement
    {
        public int SecondsBefore { get; set; }

        public string? MessageTime { get; set; }
    }
}
