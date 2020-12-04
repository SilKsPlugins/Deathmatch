using HarmonyLib;
using SDG.Unturned;
using UnityEngine;

namespace Deathmatch.Hub
{
    public class UnturnedPatches
    {
        public delegate void PositionUpdated(Player player);
        public static event PositionUpdated OnPositionUpdated;

        public delegate void Reviving(Player player, ref Vector3 position, ref byte angle);
        public static event Reviving OnReviving;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerMovement), "updateRegionAndBound")]
            [HarmonyPostfix]
            private static void UpdatePosition(PlayerMovement __instance)
            {
                OnPositionUpdated?.Invoke(__instance.player);
            }

            [HarmonyPatch(typeof(PlayerLife), "tellRevive")]
            [HarmonyPrefix]
            private static void Reviving(PlayerLife __instance, ref Vector3 position, ref byte angle)
            {
                OnReviving?.Invoke(__instance.player, ref position, ref angle);
            }
        }
    }
}
