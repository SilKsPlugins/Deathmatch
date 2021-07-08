using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches.Events;
using Deathmatch.API.Players;
using Deathmatch.API.Players.Events;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using Priority = OpenMod.API.Prioritization.Priority;

namespace Deathmatch.Core.Grace
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GraceManager : IGraceManager, IDisposable,
        IInstanceEventListener<UnturnedPlayerDamagingEvent>,
        IInstanceEventListener<IGamePlayerLeftMatchEvent>,
        IInstanceEventListener<IMatchEndedEvent>
    {
        private readonly IGamePlayerManager _playerManager;
        private readonly Dictionary<IGamePlayer, float> _gracedPlayers;

        public GraceManager(IGamePlayerManager playerManager)
        {
            _playerManager = playerManager;
            _gracedPlayers = new Dictionary<IGamePlayer, float>();

            OnEquipmentInput += Events_OnEquipmentInput;
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

            RevokeGracePeriod(player);
        }

        public UniTask HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
        {
            var player = _playerManager.GetPlayer(@event.Player);

            if (WithinGracePeriod(player))
            {
                @event.IsCancelled = true;
            }

            return UniTask.CompletedTask;
        }

        public UniTask HandleEventAsync(object? sender, IGamePlayerLeftMatchEvent @event)
        {
            RevokeGracePeriod(@event.Player);

            return UniTask.CompletedTask;
        }

        public UniTask HandleEventAsync(object? sender, IMatchEndedEvent @event)
        {
            foreach (var player in @event.Match.GetPlayers())
            {
                RevokeGracePeriod(player);
            }

            return UniTask.CompletedTask;
        }

        private delegate void EquipmentInput(Player player, bool inputPrimary, bool inputSecondary);
        private static event EquipmentInput? OnEquipmentInput;

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(PlayerEquipment), "simulate")]
            [HarmonyPrefix]
            // ReSharper disable once InconsistentNaming
            private static void Simulate(PlayerEquipment __instance, bool inputPrimary, bool inputSecondary)
            {
                OnEquipmentInput?.Invoke(__instance.player, inputPrimary, inputSecondary);
            }
        }
    }
}
