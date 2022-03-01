using MoreLinq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Helpers
{
    public static class StringHelper
    {
        public static T FindBestMatch<T>(this IEnumerable<T> enumerable, Func<T, string> termSelector, string searchString)
        {
            return enumerable.Where(x =>
                    (termSelector(x)?.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                .MinBy(asset =>
                    OpenMod.Core.Helpers.StringHelper.LevenshteinDistance(searchString, termSelector(asset)))
                .FirstOrDefault();
        }
    }
}
