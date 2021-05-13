﻿namespace Sideways
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static IEnumerable<float> MovingAverage<T>(this IEnumerable<T> xs, int period, Func<T, float> selector)
        {
            var sum = 0f;
            var buffer = new float[period];
            var n = 0;
            foreach (var x in xs)
            {
                n++;
                var value = selector(x);
                sum += value;
                var index = n % period;
                if (n >= period)
                {
                    sum -= buffer[index];
                    yield return sum / period;
                }

                buffer[index] = value;
            }
        }

        public static IEnumerable<double> MovingAverage(this IEnumerable<double> xs, int period)
        {
            var sum = 0.0;
            var buffer = new double[period];
            var n = 0;
            foreach (var x in xs)
            {
                n++;
                sum += x;
                var index = n % period;
                if (n >= period)
                {
                    sum -= buffer[index];
                    yield return sum / period;
                }

                buffer[index] = x;
            }
        }

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
