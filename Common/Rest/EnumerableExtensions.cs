using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Rest
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> input)
        {
            return input ?? Enumerable.Empty<T>();
        }
        
        public static IEnumerable<T> EmptyIfNullAndNotNull<T>(this IEnumerable<T> input)
        {
            return input.EmptyIfNull().Where(t => t != null);
        }

        public static string JoinString<T>(this IEnumerable<T> input, string separator)
        {
            return string.Join(separator, input);
        }

        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            foreach (var element in input) action.Invoke(element);
        }

        public static IEnumerable<U> Scan<T, U>(this IEnumerable<T> input, U seed, Func<U, T, U> next)
        {
            foreach (var item in input)
            {
                seed = next(seed, item);
                yield return seed;
            }
        }
    }
}