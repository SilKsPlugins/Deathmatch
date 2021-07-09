using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using HarmonyLib;
using JetBrains.Annotations;
using SDG.Unturned;

namespace Deathmatch.Addons.Addons
{
    [UsedImplicitly]
    public class NoDeathDropsAddon : AddonBase
    {
        public override string Title => "NoDeathDrops";

        private readonly IGamePlayerManager _playerManager;

        public NoDeathDropsAddon(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        protected override UniTask OnLoadAsync()
        {
            OnDeathDropInventory += Events_OnDeathDropInventory;
            OnDeathDropClothing += Events_OnDeathDropClothing;

            return UniTask.CompletedTask;
        }

        protected override UniTask OnUnloadAsync()
        {
            OnDeathDropInventory -= Events_OnDeathDropInventory;
            OnDeathDropClothing -= Events_OnDeathDropClothing;

            return UniTask.CompletedTask;
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

        public static DeathDrop? OnDeathDropClothing;
        public static DeathDrop? OnDeathDropInventory;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerClothing), "onLifeUpdated")]
            [HarmonyPrefix]
            private static void OnLifeUpdated_Clothing(PlayerClothing __instance, bool isDead)
            {
                if (isDead)
                {
                    OnDeathDropInventory?.Invoke(__instance.player);
                    OnDeathDropClothing?.Invoke(__instance.player);
                }
            }

            [HarmonyPatch(typeof(PlayerInventory), "onLifeUpdated")]
            [HarmonyPrefix]
            private static void OnLifeUpdated_Inventory(PlayerInventory __instance, bool isDead)
            {
                if (isDead)
                {
                    OnDeathDropInventory?.Invoke(__instance.player);
                }
            }
        }
    }
}
