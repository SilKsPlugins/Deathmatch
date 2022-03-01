using OpenMod.API;
using System.Collections.Generic;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadoutCategory
    {
        string Title { get; }

        IReadOnlyCollection<string> Aliases { get; }

        IOpenModComponent Component { get; }

        ILoadout? GetDefaultLoadout();

        IReadOnlyCollection<ILoadout> GetLoadouts();
    }

    public interface ILoadoutCategory<out TLoadout> : ILoadoutCategory where TLoadout : ILoadout
    {
        new TLoadout? GetDefaultLoadout();
        
        new IReadOnlyCollection<TLoadout> GetLoadouts();
    }
}
