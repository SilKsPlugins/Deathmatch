using Autofac;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Common.Helpers;
using OpenMod.Core.Ioc;
using OpenMod.Unturned.Plugins;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: PluginMetadata("Deathmatch.Addons", DisplayName = "Deathmatch Addons")]
namespace Deathmatch.Addons
{
    public class DeathmatchAddonsPlugin : OpenModUnturnedPlugin
    {
        private readonly ILogger<DeathmatchAddonsPlugin> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ILifetimeScope _lifetimeScope;

        private readonly List<IAddon> _loadedAddons;
        private readonly Dictionary<Type, List<(IAddon addon, MethodInfo method)>> _subscribedEvents;

        public DeathmatchAddonsPlugin(ILogger<DeathmatchAddonsPlugin> logger,
            IConfiguration configuration,
            IEventSubscriber eventSubscriber,
            ILifetimeScope lifetimeScope,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _lifetimeScope = lifetimeScope;
            _eventSubscriber = eventSubscriber;
            _logger = logger;

            _loadedAddons = new List<IAddon>();
            _subscribedEvents = new Dictionary<Type, List<(IAddon addon, MethodInfo method)>>();
        }

        protected override UniTask OnLoadAsync()
        {
            var addonTypes = GetType().Assembly.FindTypes<IAddon>().ToList();

            var disabledAddons = _configuration.GetSection("DisabledAddons").Get<string[]>() ?? new string[0];

            foreach (var type in addonTypes)
            {
                try
                {
                    var addon = (IAddon)ActivatorUtilitiesEx.CreateInstance(_lifetimeScope, type);

                    if (disabledAddons.Any(x => x.Equals(addon.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogInformation($"Skipping disabled addon - {addon.Title}");
                        
                        continue;
                    }

                    _eventSubscriber.Subscribe(addon, this);

                    addon.LoadAsync();

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
                    addon.UnloadAsync();

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
