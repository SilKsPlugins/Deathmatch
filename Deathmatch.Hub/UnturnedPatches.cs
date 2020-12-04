using HarmonyLib;
using SDG.Unturned;

namespace Deathmatch.Hub
{
    public class UnturnedPatches
    {
        public delegate void PositionUpdated(Player player);
        public static event PositionUpdated OnPositionUpdated;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerMovement), "updateRegionAndBound")]
            [HarmonyPostfix]
            private static void UpdatePosition(PlayerMovement __instance)
            {
                OnPositionUpdated?.Invoke(__instance.player);
            }
        }
    }
}
