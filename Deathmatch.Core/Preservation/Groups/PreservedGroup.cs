using SDG.Unturned;
using Steamworks;
using System.Reflection;

namespace Deathmatch.Core.Preservation.Groups
{
    public class PreservedGroup
    {
        private readonly CSteamID _groupId;
        private readonly EPlayerGroupRank _groupRank;
        private readonly bool _inMainGroup;

        private static readonly FieldInfo InMainGroup =
            typeof(PlayerQuests).GetField("inMainGroup", BindingFlags.Instance | BindingFlags.NonPublic);

        public PreservedGroup(PlayerQuests quests)
        {
            _groupId = quests.groupID;
            _groupRank = quests.groupRank;
            _inMainGroup = (bool) InMainGroup.GetValue(quests);

            quests.channel.send("tellSetGroup", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                CSteamID.Nil,
                (byte)0
            });
            InMainGroup.SetValue(quests, false);
        }

        public void Restore(PlayerQuests quests)
        {
            quests.channel.send("tellSetGroup", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                _groupId,
                (byte)_groupRank
            });
            InMainGroup.SetValue(quests, _inMainGroup);
        }
    }
}
