using Deathmatch.API.Loadouts;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Items;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public static class LoadoutManagerExtensions
    {
        public static ILoadoutCategory? GetCategory(this ILoadoutManager loadoutManager, Type categoryType)
        {
            var categories = loadoutManager.GetCategories();

            return categories.FirstOrDefault(categoryType.IsInstanceOfType);
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

        public static TLoadoutCategory? GetCategory<TLoadoutCategory>(this ILoadoutManager loadoutManager)
            where TLoadoutCategory : class, ILoadoutCategory
        {
            return (TLoadoutCategory?)loadoutManager.GetCategory(typeof(TLoadoutCategory));
        }

        public static async Task LoadAndAddCategory<TLoadout, TItem>(this ILoadoutManager loadoutManager,
            LoadoutCategoryBase<TLoadout, TItem> loadoutCategory)
            where TLoadout : LoadoutBase<TItem>
            where TItem : Item
        {
            await loadoutCategory.Load();

            loadoutManager.AddCategory(loadoutCategory);
        }
    }
}
