using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using MoreLinq;
using MoreLinq.Extensions;
using OpenMod.API.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public static class LoadoutExtensions
    {
        public static ILoadout? GetLoadout(this ILoadoutCategory loadoutCategory, string title, bool exact = true)
        {
            var loadouts = loadoutCategory.GetLoadouts();

            return exact
                ? loadouts.FirstOrDefault(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
                : loadouts.FindBestMatch(x => x.Title, title);
        }

        public static ILoadoutCategory? GetCategory(this ILoadoutManager loadoutManager, string title, bool exact = true)
        {
            var categories = loadoutManager.GetCategories();

            if (exact)
            {
                return categories.FirstOrDefault(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)) ??
                       categories.FirstOrDefault(x =>
                           x.Aliases.Any(y => y.Equals(title, StringComparison.OrdinalIgnoreCase)));
            }

            return categories.FindBestMatch(x => x.Title, title) ??
                   categories.SelectMany(category => category.Aliases.Select(alias => (alias, category)))
                       .FindBestMatch(pair => pair.alias, title).category;
        }

        public static async Task<ILoadout?> GetRandomLoadout(this ILoadoutManager loadoutManager, string categoryTitle,
            IGamePlayer player, IPermissionChecker? permissionChecker = null)
        {
            var category = loadoutManager.GetCategory(categoryTitle);

            if (category == null)
            {
                return null;
            }

            if (permissionChecker == null)
            {
                return category.GetLoadouts().RandomElement();
            }

            foreach (var randomLoadout in category.GetLoadouts().ToList().Shuffle())
            {
                if (randomLoadout.Permission == null ||
                    await permissionChecker.CheckPermissionAsync(player.User, randomLoadout.Permission) ==
                    PermissionGrantResult.Grant)
                {
                    return randomLoadout;
                }
            }

            return null;
        }
    }
}
