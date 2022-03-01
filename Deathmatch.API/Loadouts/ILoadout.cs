using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using OpenMod.API.Permissions;
using System.Threading.Tasks;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadout
    {
        string Title { get; }

        UniTask GiveToPlayer(IGamePlayer player);

        Task<bool> IsPermitted(IPermissionActor actor);
    }
}
