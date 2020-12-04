using OpenMod.API.Ioc;
using System.Collections.Generic;

namespace Deathmatch.API.Loadouts
{
    [Service]
    public interface ILoadoutManager
    {
        IReadOnlyCollection<ILoadoutCategory> GetCategories();
        ILoadoutCategory GetCategory(string title);

        void AddCategory(ILoadoutCategory category);

        void RemoveCategory(string title);
        void RemoveCategory(ILoadoutCategory category);
    }
}
