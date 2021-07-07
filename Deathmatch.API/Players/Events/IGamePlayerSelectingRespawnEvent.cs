using System.Numerics;

namespace Deathmatch.API.Players.Events
{
    public interface IGamePlayerSelectingRespawnEvent : IGamePlayerEvent
    {
        public bool WantsToSpawnAtHome { get; set; }

        public Vector3 Position { get; set; }

        public float Yaw { get; set; }
    }
}
