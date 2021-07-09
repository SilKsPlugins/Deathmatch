using OpenMod.API.Ioc;
using System.Collections.Generic;

namespace Deathmatch.API.Loadouts
{
    [Service]
    public interface ILoadoutManager
    {
        IReadOnlyCollection<ILoadoutCategory> GetCategories();

        void AddCategory(ILoadoutCategory category);

        void RemoveCategory(ILoadoutCategory category);
    }
}
