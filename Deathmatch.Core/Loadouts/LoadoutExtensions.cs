using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using OpenMod.API.Permissions;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public static class LoadoutExtensions
    {
        public static async Task<ILoadout> GetRandomLoadout(this ILoadoutManager loadoutManager, string categoryTitle,
            IGamePlayer player, IPermissionChecker permissionChecker = null)
        {
            var category = loadoutManager.GetCategory(categoryTitle);

            if (category == null) return null;

            if (permissionChecker == null)
                return category.GetLoadouts().RandomElement();

            foreach (var randomLoadout in category.GetLoadouts().ToList().Shuffle())
            {
                if (await permissionChecker.CheckPermissionAsync(player.User, randomLoadout.Permission) ==
                    PermissionGrantResult.Grant)
                    return randomLoadout;
            }

            return null;
        }
    }
}
