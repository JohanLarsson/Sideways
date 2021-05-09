namespace Sideways
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static T? FirstOrNull<T>(this IEnumerable<T> xs, Func<T, bool> selector)
            where T : struct
        {
            foreach (var x in xs)
            {
                if (selector(x))
                {
                    return x;
                }
            }

            return null;
        }

        public static T? LastOrNull<T>(this IEnumerable<T> xs, Func<T, bool> selector)
            where T : struct
        {
            T? match = null;
            foreach (var x in xs)
            {
                if (selector(x))
                {
                    match = x;
                }
            }

            return match;
        }
    }
}
