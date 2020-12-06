using Deathmatch.API.Players;
using OpenMod.API.Ioc;
using System.Threading.Tasks;

namespace Deathmatch.API.Loadouts
{
    [Service]
    public interface ILoadoutSelector
    {
        ILoadout GetLoadout(IGamePlayer player, string category);
        Task SetLoadout(IGamePlayer player, string category, string loadout);
    }
}
