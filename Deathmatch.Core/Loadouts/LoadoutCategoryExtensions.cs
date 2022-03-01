using Deathmatch.API.Loadouts;
using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using OpenMod.API.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Loadouts
{
    public static class LoadoutCategoryExtensions
    {
        private static ILoadout? GetLoadoutInternal(ILoadoutCategory loadoutCategory, string title, bool exact = true)
        {
            var loadouts = loadoutCategory.GetLoadouts();

            return exact
                ? loadouts.FirstOrDefault(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
                : loadouts.FindBestMatch(x => x.Title, title);
        }

        public static ILoadout? GetLoadout(this ILoadoutCategory loadoutCategory, string title, bool exact = true)
        {
            return GetLoadoutInternal(loadoutCategory, title, exact);
        }

        public static TLoadout? GetLoadout<TLoadout>(
            this ILoadoutCategory<TLoadout> loadoutCategory, string title, bool exact = true) where TLoadout : class, ILoadout
        {
            return (TLoadout?)GetLoadoutInternal(loadoutCategory, title, exact);
        }

        private static async Task<ILoadout?> GetRandomLoadoutInternal(ILoadoutCategory loadoutCategory, IGamePlayer player)
        {
            foreach (var randomLoadout in loadoutCategory.GetLoadouts().ToList().Shuffle())
            {
                if (!await randomLoadout.IsPermitted(player.User))
                {
                    continue;
                }

                return randomLoadout;
            }

            return default;
        }

        public static async Task<ILoadout?> GetRandomLoadout(this ILoadoutCategory loadoutCategory, IGamePlayer player)
        {
            return await GetRandomLoadoutInternal(loadoutCategory, player);
        }

        public static async Task<TLoadout?> GetRandomLoadout<TLoadout>(
            this ILoadoutCategory<TLoadout> loadoutCategory, IGamePlayer player) where TLoadout : class, ILoadout
        {
            return (TLoadout?)await GetRandomLoadoutInternal(loadoutCategory, player);
        }

        private static async Task<IReadOnlyCollection<ILoadout?>> GetLoadoutsInternal(ILoadoutCategory loadoutCategory, IPermissionActor actor)
        {
            var loadouts = new List<ILoadout>();

            foreach (var loadout in loadoutCategory.GetLoadouts())
            {
                if (!await loadout.IsPermitted(actor))
                {
                    continue;
                }

                loadouts.Add(loadout);
            }

            return loadouts;
        }

        public static async Task<IReadOnlyCollection<ILoadout?>> GetLoadouts(ILoadoutCategory loadoutCategory, IPermissionActor actor)
        {
            return await GetLoadoutsInternal(loadoutCategory, actor);
        }

        public static async Task<IReadOnlyCollection<TLoadout?>> GetLoadouts<TLoadout>(
            ILoadoutCategory<TLoadout> loadoutCategory, IPermissionActor actor) where TLoadout : ILoadout
        {
            return (await GetLoadoutsInternal(loadoutCategory, actor)).OfType<TLoadout>().ToList();
        }
    }
}
