using Deathmatch.API.Players;
using OpenMod.API.Ioc;

namespace Deathmatch.API.Loadouts
{
    [Service]
    public interface ILoadoutSelector
    {
        ILoadout? GetSelectedLoadout(IGamePlayer player, ILoadoutCategory category);

        void SetSelectedLoadout(IGamePlayer player, ILoadoutCategory category, ILoadout loadout);
    }
}
