using Deathmatch.API.Players;
using HarmonyLib;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.NetTransport;
using SDG.Unturned;
using System.Reflection;
using UnityEngine;

namespace Deathmatch.Core.Players.Extensions
{
    public static class PlayerRespawnExtensions
    {
        private static readonly ClientInstanceMethod<Vector3, byte> SendRevive =
            AccessTools.StaticFieldRefAccess<PlayerLife, ClientInstanceMethod<Vector3, byte>>("SendRevive");

        private static readonly FieldInfo LastRespawn = AccessTools.Field(typeof(PlayerLife), "_lastRespawn");

        public static void ForceRespawn(this Player player)
        {
            LastRespawn.SetValue(player.life, 0f);
            player.life.ReceiveRespawnRequest(false);
        }

        public static void ForceRespawn(this Player player, Vector3 position, float yaw)
        {
            var rotation = MeasurementTool.angleToByte(yaw);

            player.ForceRespawn(position, rotation);
        }

        public static void ForceRespawn(this Player player, Vector3 position, byte rotation)
        {
            player.life.sendRevive();
            SendRevive.InvokeAndLoopback(player.life.GetNetId(), ENetReliability.Reliable,
                Provider.EnumerateClients_Remote(), position, rotation);
        }

        public static void ForceRespawn(this IGamePlayer player) => ForceRespawn(player.Player);

        public static void ForceRespawn(this IGamePlayer player, Vector3 position, float yaw) =>
            ForceRespawn(player.Player, position, yaw);

        public static void ForceRespawn(this IGamePlayer player, Vector3 position, byte rotation) =>
            ForceRespawn(player.Player, position, rotation);

        public static void ForceRespawn(this UnturnedPlayer player) => ForceRespawn(player.Player);

        public static void ForceRespawn(this UnturnedPlayer player, Vector3 position, float yaw) =>
            ForceRespawn(player.Player, position, yaw);

        public static void ForceRespawn(this UnturnedPlayer player, Vector3 position, byte rotation) =>
            ForceRespawn(player.Player, position, rotation);

        public static void ForceRespawn(this UnturnedUser player) => ForceRespawn(player.Player.Player);

        public static void ForceRespawn(this UnturnedUser player, Vector3 position, float yaw) =>
            ForceRespawn(player.Player.Player, position, yaw);

        public static void ForceRespawn(this UnturnedUser player, Vector3 position, byte rotation) =>
            ForceRespawn(player.Player.Player, position, rotation);
    }
}
