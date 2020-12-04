using Deathmatch.API.Players;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using System.Collections.Generic;
using UnityEngine;

namespace Deathmatch.Core.Grace
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class GraceManager : IGraceManager
    {
        private readonly Dictionary<IGamePlayer, float> _gracedPlayers;

        public GraceManager()
        {
            _gracedPlayers = new Dictionary<IGamePlayer, float>();
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
    }
}
