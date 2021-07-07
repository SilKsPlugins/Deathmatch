using Deathmatch.API.Players;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Priority = OpenMod.API.Prioritization.Priority;

namespace Deathmatch.Core.Grace
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GraceManager : IGraceManager, IDisposable
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly Dictionary<IGamePlayer, float> _gracedPlayers;

        public GraceManager(IGamePlayerManager playerManager,
            IEventBus eventBus,
            IRuntime runtime)
        {
            _playerManager = playerManager;
            _gracedPlayers = new Dictionary<IGamePlayer, float>();

            OnEquipmentInput += Events_OnEquipmentInput;

            eventBus.Subscribe(runtime, (EventCallback<UnturnedPlayerDamagingEvent>)OnPlayerDamaging);
        }

        public void Dispose()
        {
            OnEquipmentInput -= Events_OnEquipmentInput;
        }

        public bool WithinGracePeriod(IGamePlayer player)
        {
            if (!_gracedPlayers.TryGetValue(player, out var time))
                return false;

            if (time >= Time.realtimeSinceStartup)
                return true;

            _gracedPlayers.Remove(player);
            return false;
        }

        public void GrantGracePeriod(IGamePlayer player, float seconds)
        {
            var time = Time.realtimeSinceStartup + seconds;

            if (_gracedPlayers.TryGetValue(player, out var prevTime))
            {
                _gracedPlayers[player] = time > prevTime ? time : prevTime;
            }
            else
            {
                _gracedPlayers.Add(player, time);
            }
        }

        public void RevokeGracePeriod(IGamePlayer player)
        {
            _gracedPlayers.Remove(player);
        }

        private void Events_OnEquipmentInput(Player nativePlayer, bool inputPrimary, bool inputSecondary)
        {
            var player = _playerManager.GetPlayer(nativePlayer);

            if (WithinGracePeriod(player))
                RevokeGracePeriod(player);
        }

        public Task OnPlayerDamaging(IServiceProvider serviceProvider, object? sender, UnturnedPlayerDamagingEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (WithinGracePeriod(player))
                @event.IsCancelled = true;

            return Task.CompletedTask;
        }

        private delegate void EquipmentInput(Player player, bool inputPrimary, bool inputSecondary);
        private static event EquipmentInput? OnEquipmentInput;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerEquipment), "simulate")]
            [HarmonyPrefix]
            private static void Simulate(PlayerEquipment __instance, bool inputPrimary, bool inputSecondary)
            {
                OnEquipmentInput?.Invoke(__instance.player, inputPrimary, inputSecondary);
            }
        }
    }
}
