using System.Collections.Generic;
using Deathmatch.API.Matches;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;

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
