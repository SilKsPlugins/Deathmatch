extern alias JetBrainsAnnotations;
using Autofac;
using Deathmatch.Core.Spawns;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Plugins;

namespace FreeForAll
{
    [UsedImplicitly]
    public class PluginContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.RegisterType<SpawnDirectory>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
