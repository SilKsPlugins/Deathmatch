using Deathmatch.API.Players;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Players
{
    public static class PlayerExtensions
    {
        private const string TeamKey = "Team";

        public static Team GetTeam(this IGamePlayer player) => player.GetMatchData<Team>(TeamKey);

        public static void SetTeam(this IGamePlayer player, Team team) => player.SetMatchData(TeamKey, team);
    }
}
