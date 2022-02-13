using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.Addons.API;
using Deathmatch.Addons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Common.Helpers;
using OpenMod.Core.Events;
using OpenMod.Core.Ioc;
using SilK.Unturned.Extras.Configuration;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Addons
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class AddonsActivator : IAddonsActivator, IAsyncDisposable,
        IInstanceEventListener<OpenModInitializedEvent>
    {

        private readonly IConfigurationParser<DeathmatchAddonsConfig> _configuration;
        private readonly ILogger<AddonsActivator> _logger;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IOpenModComponent _component;

        private readonly List<IAddon> _loadedAddons;

        public AddonsActivator(IConfigurationParser<DeathmatchAddonsConfig> configuration,
            ILogger<AddonsActivator> logger,
            IEventSubscriber eventSubscriber,
            ILifetimeScope lifetimeScope,
            IOpenModComponent component)
        {
            _configuration = configuration;
            _logger = logger;
            _eventSubscriber = eventSubscriber;
            _lifetimeScope = lifetimeScope;
            _component = component;

            _loadedAddons = new();
        }

        public async UniTask HandleEventAsync(object? sender, OpenModInitializedEvent @event)
        {
            var addonTypes = GetType().Assembly.FindTypes<IAddon>().ToList();

            var disabledAddons = _configuration.Instance.DisabledAddons;

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

                    _eventSubscriber.Subscribe(addon, _component);

                    await addon.LoadAsync();

                    _loadedAddons.Add(addon);

                    _logger.LogInformation($"Loaded addon - {addon.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while loading addon ({type.Name})");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var addon in _loadedAddons)
            {
                try
                {
                    await addon.UnloadAsync();

                    _logger.LogInformation($"Unloaded addon - {addon.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while unloading addon ({addon.Title})");
                }
            }

            _loadedAddons.Clear();
        }
    }
}
