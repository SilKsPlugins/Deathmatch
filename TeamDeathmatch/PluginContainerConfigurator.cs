extern alias JetBrainsAnnotations;
using Autofac;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Plugins;
using TeamDeathmatch.Spawns;

namespace TeamDeathmatch
{
    [UsedImplicitly]
    public class PluginContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.RegisterType<BlueSpawnDirectory>()
                .AsSelf()
                .SingleInstance();

            context.ContainerBuilder.RegisterType<RedSpawnDirectory>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
