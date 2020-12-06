using OpenMod.API;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadoutCategory
    {
        string Title { get; }

        IReadOnlyCollection<string> Aliases { get; }

        IOpenModComponent Component { get; }

        ILoadout GetLoadout(string title);
        IReadOnlyCollection<ILoadout> GetLoadouts();

        Task SaveLoadouts();

        void AddLoadout(ILoadout loadout);
        bool RemoveLoadout(ILoadout loadout);
    }
}
