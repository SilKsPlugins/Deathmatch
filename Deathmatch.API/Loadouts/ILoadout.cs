using Deathmatch.API.Players;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadout
    {
        string Title { get; }

        string Permission { get; }

        void GiveToPlayer(IGamePlayer player);
    }
}
