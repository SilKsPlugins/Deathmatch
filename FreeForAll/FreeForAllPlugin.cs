using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.Core.Loadouts;
using FreeForAll.Loadouts;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("FreeForAll", DisplayName = "Free For All")]
namespace FreeForAll
{
    public class FreeForAllPlugin : OpenModUnturnedPlugin
    {
        public const string SpawnsKey = "spawns";
        
        private readonly ILoadoutManager _loadoutManager;
        private readonly IServiceProvider _serviceProvider;

        public FreeForAllPlugin(ILoadoutManager loadoutManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _loadoutManager = loadoutManager;
            _serviceProvider = serviceProvider;
        }

        protected override async UniTask OnLoadAsync()
        {
            await _loadoutManager.LoadAndAddCategory(new FFALoadoutCategory(_serviceProvider));
        }
    }
}
