using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Deathmatch.Hub
{
    [Serializable]
    public class Hub
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Yaw { get; set; }

        public float Radius { get; set; }

        public Vector2 GetVector2() => new Vector2(X, Z);

        public Vector2 GetVector3() => new Vector3(X, Y, Z);

        public float DistSqr(Vector3 position)
        {
            var other = new Vector2(position.x, position.z);

            return (GetVector2() - other).sqrMagnitude;
        }

        public void TeleportPlayer(Player player)
        {
            player.teleportToLocationUnsafe(GetVector3(), Yaw);
        }

        public Hub()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Yaw = 0;
            Radius = 10;
        }

        public Hub(UnturnedUser user, float radius)
        {
            var pos = user.Player.Player.transform.position;

            X = pos.x;
            Y = pos.y;
            Z = pos.z;

            Yaw = user.Player.Player.transform.eulerAngles.y;

            Radius = radius;
        }
    }
}
