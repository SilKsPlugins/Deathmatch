using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using OpenMod.API.Ioc;

namespace Deathmatch.API.Preservation
{
    [Service]
    public interface IPreservationManager
    {
        UniTask PreservePlayer(IGamePlayer player);
        UniTask RestorePlayer(IGamePlayer player);
        UniTask<bool> IsPreserved(IGamePlayer player);

        UniTask RestoreAll();
    }
}
