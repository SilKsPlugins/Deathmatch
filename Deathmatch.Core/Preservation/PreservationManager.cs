using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using Deathmatch.API.Preservation;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Priority = OpenMod.API.Prioritization.Priority;

namespace Deathmatch.Core.Preservation
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class PreservationManager : IPreservationManager, IDisposable
    {
        private readonly ILogger<PreservationManager> _logger;
        private readonly List<PreservedPlayer> _preservedPlayers;

        public PreservationManager(ILogger<PreservationManager> logger)
        {
            _logger = logger;
            _preservedPlayers = new List<PreservedPlayer>();

            OnPlayerSaving += Events_OnPlayerSaving;
            Provider.onEnemyDisconnected += OnEnemyDisconnected;
        }

        private PreservedPlayer GetPreservedPlayer(CSteamID steamId) =>
            _preservedPlayers.FirstOrDefault(x => x.SteamId == steamId);

        public async UniTask PreservePlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            if (GetPreservedPlayer(player.SteamId) != null)
                throw new Exception("Player already has preserved instance");

            _preservedPlayers.Add(new PreservedPlayer(player));
        }

        public async UniTask RestorePlayer(IGamePlayer player)
        {
            await UniTask.SwitchToMainThread();

            var preservedPlayer = GetPreservedPlayer(player.SteamId);

            if (preservedPlayer == null) return;

            _preservedPlayers.Remove(preservedPlayer);

            preservedPlayer.Restore();
        }

        public UniTask<bool> IsPreserved(IGamePlayer player)
        {
            return UniTask.FromResult(GetPreservedPlayer(player.SteamId) != null);
        }

        public async UniTask RestoreAll()
        {
            await UniTask.SwitchToMainThread();

            foreach (var preservedPlayer in _preservedPlayers)
            {
                if (preservedPlayer == null) continue;

                try
                {
                    preservedPlayer.Restore();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown when restoring player " + preservedPlayer.SteamId);
                }
            }

            _preservedPlayers.Clear();
        }

        private bool _isDisposing;

        public void Dispose()
        {
            if (_isDisposing) return;

            _isDisposing = true;

            OnPlayerSaving -= Events_OnPlayerSaving;

            // ReSharper disable once DelegateSubtraction
            Provider.onEnemyDisconnected -= OnEnemyDisconnected;
        }

        private void OnEnemyDisconnected(SteamPlayer player)
        {
            _preservedPlayers.RemoveAll(x => x.SteamId == player.playerID.steamID);
        }

        private void Events_OnPlayerSaving(Player player, ref bool cancel)
        {
            if (GetPreservedPlayer(player.channel.owner.playerID.steamID) != null)
            {
                cancel = true;
            }
        }

        private delegate void PlayerSaving(Player player, ref bool cancel);
        private static event PlayerSaving OnPlayerSaving;

        [HarmonyPatch]
        // ReSharper disable UnusedType.Local
        private static class SavePatches
        {
            [HarmonyPatch(typeof(Player), "save")]
            [HarmonyPrefix]
            private static bool PreSave(Player __instance)
            {
                bool cancel = false;

                OnPlayerSaving?.Invoke(__instance, ref cancel);

                return !cancel;
            }
        }
        // ReSharper disable restore UnusedType.Local
    }
}
