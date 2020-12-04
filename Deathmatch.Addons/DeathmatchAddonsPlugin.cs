using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[assembly: PluginMetadata("Deathmatch.Addons", DisplayName = "Deathmatch Addons")]
namespace Deathmatch.Addons
{
    public class DeathmatchAddonsPlugin : OpenModUnturnedPlugin
    {
        private readonly ILogger<DeathmatchAddonsPlugin> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEventBus _eventBus;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<IAddon> _loadedAddons;
        private readonly Dictionary<Type, List<(IAddon addon, MethodInfo method)>> _subscribedEvents;

        public DeathmatchAddonsPlugin(ILogger<DeathmatchAddonsPlugin> logger,
            IConfiguration configuration,
            IEventBus eventBus,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _eventBus = eventBus;
            _logger = logger;

            _loadedAddons = new List<IAddon>();
            _subscribedEvents = new Dictionary<Type, List<(IAddon addon, MethodInfo method)>>();
        }

        protected override UniTask OnLoadAsync()
        {
            var addonTypes = GetType().Assembly.FindTypes<IAddon>(false).ToList();

            var disabledAddons = _configuration.GetSection("DisabledAddons").Get<string[]>();

            foreach (var type in addonTypes)
            {
                try
                {
                    var addon = (IAddon)ActivatorUtilities.CreateInstance(_serviceProvider, type);

                    if (disabledAddons.Any(x => x.Equals(addon.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogInformation($"Skipping disabled addon - {addon.Title}");
                        continue;
                    }

                    addon.Load();

                    // Custom event implementation

                    var eventListeners = type.GetInterfaces().Where(x =>
                        x.IsGenericType && x.GetGenericTypeDefinition().IsAssignableFrom(typeof(IAddonEventListener<>)));

                    foreach (var listener in eventListeners)
                    {
                        var eventType = listener.GetGenericArguments().Single();

                        if (!_subscribedEvents.ContainsKey(eventType))
                        {
                            _subscribedEvents.Add(eventType, new List<(IAddon addon, MethodInfo method)>());

                            _eventBus.Subscribe(this, eventType, HandleEventAsync);
                        }

                        _subscribedEvents[eventType].Add((addon, listener.GetMethod("HandleEventAsync", BindingFlags.Public | BindingFlags.Instance)));
                    }

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

        public async Task HandleEventAsync(IServiceProvider serviceProvider, object sender, IEvent @event)
        {
            if (_subscribedEvents.TryGetValue(@event.GetType(), out var methods))
            {
                foreach (var (addon, method) in methods)
                {
                    await (Task)method.Invoke(addon, new[] { sender, @event });
                }
            }
        }
    }
}
