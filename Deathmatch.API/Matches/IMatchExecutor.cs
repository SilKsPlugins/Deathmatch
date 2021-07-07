using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using OpenMod.API.Ioc;
using System.Collections.Generic;

namespace Deathmatch.API.Matches
{
    [Service]
    public interface IMatchExecutor
    {
        IMatch? CurrentMatch { get; }

        IReadOnlyCollection<IGamePlayer> GetParticipants();
        UniTask AddParticipant(IGamePlayer player);
        UniTask RemoveParticipant(IGamePlayer user);

        UniTask<bool> StartMatch(IMatchRegistration? registration = null);
        UniTask<bool> EndMatch();
    }
}
