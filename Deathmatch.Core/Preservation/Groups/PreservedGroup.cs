using SDG.Unturned;
using Steamworks;

namespace Deathmatch.Core.Preservation.Groups
{
    public class PreservedGroup
    {
        private readonly CSteamID _groupId;
        private readonly EPlayerGroupRank _groupRank;

        public PreservedGroup(PlayerQuests quests)
        {
            _groupId = quests.groupID;
            _groupRank = quests.groupRank;

            quests.leaveGroup(true);
        }

        public void Restore(PlayerQuests quests)
        {
            quests.ServerAssignToGroup(_groupId, _groupRank, true);
        }
    }
}
