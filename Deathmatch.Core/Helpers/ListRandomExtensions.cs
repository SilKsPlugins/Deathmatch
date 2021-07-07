using System;
using System.Collections.Generic;

namespace Deathmatch.Core.Helpers
{
    public static class ListRandomExtensions
    {
        private static readonly Random Rng = new();

        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            var current = default(T);

            var count = 0;

            foreach (var element in source)
            {
                count++;
                if (Rng.Next(count) == 0)
                {
                    current = element;
                }
            }

            if (count == 0 || current == null)
            {
                throw new InvalidOperationException("Sequence was empty");
            }

            return current;
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var copy = new T[list.Count];

            list.CopyTo(copy, 0);

            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = copy[k];
                copy[k] = copy[n];
                copy[n] = value;
            }

            return copy;
        }
    }
}
