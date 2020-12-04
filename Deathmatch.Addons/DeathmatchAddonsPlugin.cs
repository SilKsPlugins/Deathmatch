using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: PluginMetadata("Deathmatch.Addons", DisplayName = "Deathmatch Addons")]
namespace Deathmatch.Addons
{
    public class DeathmatchAddonsPlugin : OpenModUnturnedPlugin
    {
        private readonly ILogger<DeathmatchAddonsPlugin> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<IAddon> _loadedAddons;

        public DeathmatchAddonsPlugin(ILogger<DeathmatchAddonsPlugin> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _loadedAddons = new List<IAddon>();
        }

        protected override UniTask OnLoadAsync()
        {
            var addonTypes = GetType().Assembly.FindTypes<IAddon>(false).ToList();

            var disabledAddons = _configuration.GetSection("DisabledAddons").Get<string[]>();

            foreach (var type in addonTypes)
            {
                try
                {
                    var addon = (IAddon)ActivatorUtilities.CreateInstance(_serviceProvider, type, this);

                    if (disabledAddons.Any(x => x.Equals(addon.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogInformation($"Skipping disabled addon - {addon.Title}");
                        continue;
                    }

                    addon.Load();

                    _loadedAddons.Add(addon);

                    _logger.LogInformation($"Loaded addon - {addon.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while loading addon ({type.Name})");
                }
            }

            return UniTask.CompletedTask;
        }

        protected override UniTask OnUnloadAsync()
        {
            foreach (var addon in _loadedAddons)
            {
                try
                {
                    addon.Unload();

                    _logger.LogInformation($"Unloaded addon - {addon.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while unloading addon ({addon.Title})");
                }
            }

            _loadedAddons.Clear();

            return UniTask.CompletedTask;
        }
    }
}
