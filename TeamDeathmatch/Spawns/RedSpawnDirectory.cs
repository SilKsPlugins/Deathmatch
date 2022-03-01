using Deathmatch.Core.Spawns;
using System;

namespace TeamDeathmatch.Spawns
{
    public class RedSpawnDirectory : SpawnDirectory
    {
        public RedSpawnDirectory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string DataStoreKey => "spawns.red";
    }
}
