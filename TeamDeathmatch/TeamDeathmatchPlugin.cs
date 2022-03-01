using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.Core.Loadouts;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;
using TeamDeathmatch.Loadouts;

[assembly: PluginMetadata("TeamDeathmatch", DisplayName = "Team Deathmatch")]
namespace TeamDeathmatch
{
    public class TeamDeathmatchPlugin : OpenModUnturnedPlugin
    {
        private readonly ILoadoutManager _loadoutManager;
        private readonly IServiceProvider _serviceProvider;

        public TeamDeathmatchPlugin(IServiceProvider serviceProvider,
            ILoadoutManager loadoutManager) : base(serviceProvider)
        {
            _loadoutManager = loadoutManager;
            _serviceProvider = serviceProvider;
        }

        protected override async UniTask OnLoadAsync()
        {
            await _loadoutManager.LoadAndAddCategory(new TDMLoadoutCategory(_serviceProvider));
        }
    }
}
