using System;

namespace FreeForAll.Configuration
{
    [Serializable]
    public class RewardConfig
    {
        public string Id { get; set; } = "";

        public int Amount { get; set; } = 1;

        public int Chance { get; set; } = 1;
    }
}
