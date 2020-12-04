using Deathmatch.API.Players;
using OpenMod.API.Ioc;

namespace Deathmatch.Core.Grace
{
    [Service]
    public interface IGraceManager
    {
        bool WithinGracePeriod(IGamePlayer player);

        void GrantGracePeriod(IGamePlayer player, float seconds);

        void RevokeGracePeriod(IGamePlayer player);
    }
}
