using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace Deathmatch.Addons.Addons
{
    public class NoDurabilityAddon : IAddon
    {
        public string Title => "NoDurability";

        private readonly ILogger<NoDurabilityAddon> _logger;

        private readonly FieldInfo _barrelDurability;
        private readonly List<(ItemBarrelAsset asset, byte durability)> _barrelAssets;

        public NoDurabilityAddon(ILogger<NoDurabilityAddon> logger)
        {
            _logger = logger;

            _barrelDurability =
                typeof(ItemBarrelAsset).GetField("_durability", BindingFlags.Instance | BindingFlags.NonPublic);
            _barrelAssets = new List<(ItemBarrelAsset asset, byte durability)>();
        }
        
        public void Load()
        {
            Level.onPostLevelLoaded += OnLevelLoaded;
            if (Level.isLoaded)
                OnLevelLoaded(0);
        }

        public void Unload()
        {
            // ReSharper disable once DelegateSubtraction
            Level.onPostLevelLoaded -= OnLevelLoaded;

            foreach (var (asset, durability) in _barrelAssets)
            {
                _barrelDurability.SetValue(asset, durability);
            }
        }

        private void OnLevelLoaded(int level)
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>();
            
            if (_barrelDurability == null)
            {
                _logger.LogWarning("Cannot find barrel durability field, cannot change durability costs.");
            }
            else
            {
                var barrelAssets = itemAssets.OfType<ItemBarrelAsset>();

                foreach (var asset in barrelAssets)
                {
                    _barrelAssets.Add((asset, asset.durability));

                    _barrelDurability.SetValue(asset, (byte)0);
                }
            }
        }
    }
}
