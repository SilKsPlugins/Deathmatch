using System;
using System.Collections.Generic;

namespace Deathmatch.Core.Helpers
{
    public static class ListRandomExtensions
    {
        private static readonly Random _rng = new Random();

        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (_rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            T[] copy = new T[list.Count];

            list.CopyTo(copy, 0);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = copy[k];
                copy[k] = copy[n];
                copy[n] = value;
            }

            return copy;
        }
    }
}
