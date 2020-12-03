using SDG.Unturned;

namespace Deathmatch.Core.Preservation.Life
{
    public class PreservedLife
    {
        private readonly byte _health;
        private readonly byte _food;
        private readonly byte _water;
        private readonly byte _stamina;
        private readonly byte _virus;
        private readonly bool _isBleeding;
        private readonly bool _isBroken;

        public PreservedLife(PlayerLife life)
        {
            _health = life.health;
            _food = life.food;
            _water = life.water;
            _stamina = life.stamina;
            _virus = life.virus;
            _isBleeding = life.isBleeding;
            _isBroken = life.isBroken;
        }

        public void Restore(PlayerLife life)
        {
            life.serverModifyHealth(_health - life.health);
            life.serverModifyFood(_food - life.food);
            life.serverModifyWater(_water - life.water);
            life.serverModifyStamina(_stamina - life.stamina);
            life.serverModifyVirus(_virus - life.virus);
            life.serverSetBleeding(_isBleeding);
            life.serverSetLegsBroken(_isBroken);
        }
    }
}
