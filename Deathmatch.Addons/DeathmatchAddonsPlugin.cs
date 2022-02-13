extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("Deathmatch.Addons", DisplayName = "Deathmatch Addons")]

namespace Deathmatch.Addons
{
    [UsedImplicitly]
    public class DeathmatchAddonsPlugin : OpenModUnturnedPlugin
    {
        public DeathmatchAddonsPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
