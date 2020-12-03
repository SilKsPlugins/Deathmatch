using Deathmatch.API.Players;

namespace FreeForAll.Players
{
    public static class PlayerExtensions
    {
        private const string KillsKey = "Kills";

        public static int GetKills(this IGamePlayer player) => player.GetMatchData<int>(KillsKey);

        public static void SetKills(this IGamePlayer player, int kills) => player.SetMatchData(KillsKey, kills);
    }
}
