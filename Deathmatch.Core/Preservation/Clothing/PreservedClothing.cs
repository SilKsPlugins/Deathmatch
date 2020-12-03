using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDG.Unturned;

namespace Deathmatch.Core.Preservation.Clothing
{
    public class PreservedClothing
    {
        private readonly ushort _backpack;
        private readonly byte _backpackQuality;
        private readonly byte[] _backpackState;

        private readonly ushort _glasses;
        private readonly byte _glassesQuality;
        private readonly byte[] _glassesState;

        private readonly ushort _hat;
        private readonly byte _hatQuality;
        private readonly byte[] _hatState;

        private readonly ushort _mask;
        private readonly byte _maskQuality;
        private readonly byte[] _maskState;

        private readonly ushort _pants;
        private readonly byte _pantsQuality;
        private readonly byte[] _pantsState;

        private readonly ushort _shirt;
        private readonly byte _shirtQuality;
        private readonly byte[] _shirtState;

        private readonly ushort _vest;
        private readonly byte _vestQuality;
        private readonly byte[] _vestState;

        public PreservedClothing(PlayerClothing clothing)
        {
            _backpack = clothing.backpack;
            _backpackQuality = clothing.backpackQuality;
            _backpackState = clothing.backpackState;

            _glasses = clothing.glasses;
            _glassesQuality = clothing.glassesQuality;
            _glassesState = clothing.glassesState;

            _hat = clothing.hat;
            _hatQuality = clothing.hatQuality;
            _hatState = clothing.hatState;

            _mask = clothing.mask;
            _maskQuality = clothing.maskQuality;
            _maskState = clothing.maskState;

            _pants = clothing.pants;
            _pantsQuality = clothing.pantsQuality;
            _pantsState = clothing.pantsState;

            _shirt = clothing.shirt;
            _shirtQuality = clothing.shirtQuality;
            _shirtState = clothing.shirtState;

            _vest = clothing.vest;
            _vestQuality = clothing.vestQuality;
            _vestState = clothing.vestState;
        }

        public void Restore(PlayerClothing clothing) => clothing.updateClothes(
            _shirt, _shirtQuality, _shirtState,
            _pants, _pantsQuality, _pantsState,
            _hat, _hatQuality, _hatState,
            _backpack, _backpackQuality, _backpackState,
            _vest, _vestQuality, _vestState,
            _mask, _maskQuality, _maskState,
            _glasses, _glassesQuality, _glassesState);
    }
}
