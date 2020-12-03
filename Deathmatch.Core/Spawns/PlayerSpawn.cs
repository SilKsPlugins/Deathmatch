﻿using Deathmatch.API.Players;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Deathmatch.Core.Spawns
{
    [Serializable]
    public class PlayerSpawn
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Yaw { get; set; }

        public Vector3 ToVector3() => new Vector3(X, Y, Z);

        public PlayerSpawn()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Yaw = 0;
        }

        public PlayerSpawn(float x, float y, float z, float yaw)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
        }

        public PlayerSpawn(Vector3 position, float yaw) : this(position.x, position.y, position.z, yaw)
        {
        }

        public PlayerSpawn(Transform transform) : this(transform.position, transform.eulerAngles.y)
        {
        }

        public PlayerSpawn(Player player) : this(player.transform)
        {
        }

        public PlayerSpawn(UnturnedPlayer player) : this(player.Player)
        {
        }

        public PlayerSpawn(UnturnedUser user) : this(user.Player)
        {
        }

        public PlayerSpawn(IGamePlayer player) : this(player.Player)
        {
        }

        public void SpawnPlayer(Player player)
        {
            if (player.life.isDead)
            {
                byte b = MeasurementTool.angleToByte(Yaw);

                player.life.sendRevive();
                player.life.channel.send("tellRevive", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    ToVector3(),
                    b
                });
            }
            else
            {
                player.teleportToLocationUnsafe(ToVector3(), Yaw);
            }
        }

        public void SpawnPlayer(UnturnedPlayer player) => SpawnPlayer(player.Player);

        public void SpawnPlayer(UnturnedUser user) => SpawnPlayer(user.Player);

        public void SpawnPlayer(IGamePlayer player) => SpawnPlayer(player.Player);
    }
}
