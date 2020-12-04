using System.Collections.Generic;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadoutCategory
    {
        string Title { get; }

        List<ILoadout> GetLoadouts();
    }
}
