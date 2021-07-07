using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using System.Numerics;

namespace Deathmatch.Core.Players.Events
{
    public class GamePlayerSelectingRespawnEvent : GamePlayerEvent, IGamePlayerSelectingRespawnEvent
    {
        public bool WantsToSpawnAtHome { get; set; }

        public Vector3 Position { get; set; }

        public float Yaw { get; set; }

        public GamePlayerSelectingRespawnEvent(IGamePlayer player, bool wantsToSpawnAtHome, Vector3 position, float yaw)
            : base(player)
        {
            WantsToSpawnAtHome = wantsToSpawnAtHome;
            Position = position;
            Yaw = yaw;
        }
    }
}
