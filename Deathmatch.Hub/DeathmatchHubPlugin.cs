using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

[assembly: PluginMetadata("Deathmatch.Hub", DisplayName = "Deathmatch Hub")]
namespace Deathmatch.Hub
{
    public class DeathmatchHubPlugin : OpenModUnturnedPlugin
    {
        private const string HubKey = "hub";

        private readonly IGamePlayerManager _playerManager;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IServiceProvider _serviceProvider;

        public Hub Hub { get; private set; }

        public DeathmatchHubPlugin(IGamePlayerManager playerManager,
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
            _serviceProvider = serviceProvider;
        }

        protected override async UniTask OnLoadAsync()
        {
            await LoadHub();

            UnturnedPatches.OnPositionUpdated += OnPositionUpdated;
        }

        protected override async UniTask OnUnloadAsync()
        {
            UnturnedPatches.OnPositionUpdated -= OnPositionUpdated;
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

            if (player.CurrentMatch != null) return;

            if (Hub.DistSqr(player.Transform.position) > Hub.Radius * Hub.Radius)
            {
                Hub.TeleportPlayer(player.Player);
            }
        }
    }
}
