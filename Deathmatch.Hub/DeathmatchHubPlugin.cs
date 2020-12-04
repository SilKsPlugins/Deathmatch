using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using UnityEngine;

[assembly: PluginMetadata("Deathmatch.Hub", DisplayName = "Deathmatch Hub")]
namespace Deathmatch.Hub
{
    public class DeathmatchHubPlugin : OpenModUnturnedPlugin
    {
        private const string HubKey = "hub";

        private readonly IGamePlayerManager _playerManager;
        private readonly IPermissionRegistry _permissionRegistry;
        private readonly IPermissionChecker _permissionChecker;

        public Hub Hub { get; private set; }

        public DeathmatchHubPlugin(IGamePlayerManager playerManager,
            IPermissionRegistry permissionRegistry,
            IPermissionChecker permissionChecker,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _permissionRegistry = permissionRegistry;
            _permissionChecker = permissionChecker;
        }

        protected override async UniTask OnLoadAsync()
        {
            await LoadHub();

            _permissionRegistry.RegisterPermission(this, "bypass", "Bypass the hub boundary.");

            UnturnedPatches.OnPositionUpdated += OnPositionUpdated;
            UnturnedPatches.OnReviving += OnReviving;
        }

        protected override async UniTask OnUnloadAsync()
        {
            UnturnedPatches.OnPositionUpdated -= OnPositionUpdated;
            UnturnedPatches.OnReviving -= OnReviving;
        }

        public async Task<Hub> LoadHub()
        {
            if (!await DataStore.ExistsAsync(HubKey)) return null;

            Hub = await DataStore.LoadAsync<Hub>(HubKey);

            return Hub;
        }

        public async Task SaveHub(Hub hub)
        {
            await DataStore.SaveAsync(HubKey, hub);

            Hub = hub;
        }

        private void OnPositionUpdated(Player nativePlayer)
        {
            if (Hub == null) return;

            var player = _playerManager.GetPlayer(x => x.SteamId == nativePlayer.channel.owner.playerID.steamID);

            // Could happen before player is registered
            if (player == null) return;

            if (player.CurrentMatch != null) return;

            if (Hub.DistSqr(player.Transform.position) > Hub.Radius * Hub.Radius &&
                AsyncHelper.RunSync(() =>
                    _permissionChecker.CheckPermissionAsync(player.User, "Deathmatch.Hub:bypass")) !=
                PermissionGrantResult.Grant)
            {
                Hub.TeleportPlayer(player.Player);
            }
        }

        private void OnReviving(Player nativePlayer, ref Vector3 position, ref byte angle)
        {
            if (Hub == null) return;

            var player = _playerManager.GetPlayer(x => x.SteamId == nativePlayer.channel.owner.playerID.steamID);

            // Could happen before player is registered
            if (player == null) return;

            if (player.CurrentMatch != null) return;

            position = Hub.GetVector3();
            angle = MeasurementTool.angleToByte(Hub.Yaw);
        }
    }
}
