using SDG.Unturned;

namespace Deathmatch.Core.Preservation.Skills
{
    public class PreservedSkills
    {
        private readonly uint _experience;
        private readonly int _reputation;
        private readonly byte[][] _skillLevels;

        public PreservedSkills(PlayerSkills skills)
        {
            _experience = skills.experience;
            _reputation = skills.reputation;

            _skillLevels = new byte[skills.skills.Length][];

            for (int i = 0; i < skills.skills.Length; i++)
            {
                _skillLevels[i] = new byte[skills.skills[i].Length];

                for (int j = 0; j < skills.skills[i].Length; j++)
                {
                    _skillLevels[i][j] = skills.skills[i][j].level;
                }
            }
        }

        public void Restore(PlayerSkills skills)
        {
            if (_experience > skills.experience)
            {
                skills.askAward(_experience - skills.experience);
            }
            else if (_experience < skills.experience)
            {
                skills.askAward(skills.experience - _experience);
            }

            skills.askRep(_reputation - skills.reputation);

            for (int i = 0; i < _skillLevels.Length; i++)
            {
                for (int j = 0; j < _skillLevels[i].Length; j++)
                {
                    skills.skills[i][j].level = _skillLevels[i][j];
                }
            }

            skills.askSkills(skills.channel.owner.playerID.steamID);
        }
    }
}
