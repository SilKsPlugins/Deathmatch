using System;

namespace Deathmatch.Core.Configuration
{
    [Serializable]
    public class DeathmatchConfig
    {
        public float MatchInterval { get; set; } = 1800;

        public AutoAnnouncement[] AutoAnnouncements { get; set; } = new AutoAnnouncement[0];

        public string[] DisabledCommands { get; set; } = new string[0];
    }
}
