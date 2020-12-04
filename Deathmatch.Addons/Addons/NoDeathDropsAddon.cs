using Deathmatch.API.Players;
using HarmonyLib;
using SDG.Unturned;

namespace Deathmatch.Addons.Addons
{
    public class NoDeathDropsAddon : IAddon
    {
        private readonly IGamePlayerManager _playerManager;

        public NoDeathDropsAddon(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public string Title => "NoDeathDrops";

        public void Load()
        {
            OnDeathDropInventory += Events_OnDeathDropInventory;
            OnDeathDropClothing += Events_OnDeathDropClothing;
        }

        public void Unload()
        {
            // ReSharper disable once DelegateSubtraction
            OnDeathDropInventory -= Events_OnDeathDropInventory;
        }

        private void Events_OnDeathDropInventory(Player nativePlayer)
        {
            var player = _playerManager.GetPlayer(nativePlayer);

            if (player.IsInActiveMatch())
            {
                player.ClearInventory();
            }
        }

        private void Events_OnDeathDropClothing(Player nativePlayer)
        {
            var player = _playerManager.GetPlayer(nativePlayer);

            if (player.IsInActiveMatch())
            {
                player.ClearClothing();
            }
        }

        public delegate void DeathDrop(Player player);

        public static DeathDrop OnDeathDropClothing;
        public static DeathDrop OnDeathDropInventory;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerClothing), "onLifeUpdated")]
            [HarmonyPrefix]
            private static void OnLifeUpdated_Clothing(PlayerClothing __instance, bool isDead)
            {
                if (isDead)
                    OnDeathDropClothing?.Invoke(__instance.player);
            }

            [HarmonyPatch(typeof(PlayerInventory), "onLifeUpdated")]
            [HarmonyPrefix]
            private static void OnLifeUpdated_Inventory(PlayerInventory __instance, bool isDead)
            {
                if (isDead)
                    OnDeathDropInventory?.Invoke(__instance.player);
            }
        }
    }
}
