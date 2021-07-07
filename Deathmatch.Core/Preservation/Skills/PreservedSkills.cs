using HarmonyLib;
using SDG.NetTransport;
using SDG.Unturned;
using System.Reflection;

namespace Deathmatch.Core.Preservation.Skills
{
    public class PreservedSkills
    {
        private readonly uint _experience;
        private readonly int _reputation;
        private readonly byte[][] _skillLevels;

        private static readonly ClientInstanceMethod SendMultipleSkillLevels =
            AccessTools.StaticFieldRefAccess<PlayerSkills, ClientInstanceMethod>("SendMultipleSkillLevels");

        private static readonly MethodInfo WriteSkillLevels =
            AccessTools.Method(typeof(PlayerSkills), "WriteSkillLevels");

        public PreservedSkills(PlayerSkills skills)
        {
            _experience = skills.experience;
            _reputation = skills.reputation;

            _skillLevels = new byte[skills.skills.Length][];

            for (var i = 0; i < skills.skills.Length; i++)
            {
                _skillLevels[i] = new byte[skills.skills[i].Length];

                for (var j = 0; j < skills.skills[i].Length; j++)
                {
                    _skillLevels[i][j] = skills.skills[i][j].level;
                }
            }
        }

        public void Restore(PlayerSkills skills)
        {
            skills.ServerSetExperience(_experience);

            skills.askRep(_reputation - skills.reputation);

            for (var i = 0; i < _skillLevels.Length; i++)
            {
                for (var j = 0; j < _skillLevels[i].Length; j++)
                {
                    skills.skills[i][j].level = _skillLevels[i][j];
                }
            }

            SendMultipleSkillLevels.InvokeAndLoopback(skills.GetNetId(), ENetReliability.Reliable,
                Provider.EnumerateClients_Remote(), x => WriteSkillLevels.Invoke(skills, new object[] {x}));
        }
    }
}
