using Deathmatch.Core.Spawns;
using System;

namespace TeamDeathmatch.Spawns
{
    public class BlueSpawnDirectory : SpawnDirectory
    {
        public BlueSpawnDirectory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string DataStoreKey => "spawns.blue";
    }
}
