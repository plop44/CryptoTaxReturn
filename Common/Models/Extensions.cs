using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Resolution;

namespace Common.Models
{
    public static class Extensions
    {
        public static DateTime ToTimeReadable(this long time)=> DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
        public static long ToUnixDateTime(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static long AddDays(this long time, int days)
        {
            if(days>0)
                return time + (long)TimeSpan.FromDays(days).TotalMilliseconds;
            
            return time - (long)TimeSpan.FromDays(-days).TotalMilliseconds;
        }

        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static IEnumerable<(TTag tag, TInput value)> Tag<TTag, TInput>(this IEnumerable<TInput> input, Func<TInput, TTag> tagExtract) =>
            input.Select(t => (tagExtract.Invoke(t), t));

        public static IEnumerable<TInput> Do<TInput>(this IEnumerable<TInput> input, Action<TInput> action) =>
            input.Select(t =>
            {
                action.Invoke(t);
                return t;
            });

        public static  decimal CleanValueTo8Decimals(this decimal value)
        {
            // we only keep 8 decimals, this code has been tested against real data (not really logic)
            var precision = value % 0.00000001m;

            if (precision != 0)
            {
                var cleanValueTo8Decimals = value - precision;
                // that is to remove the trailing zeros
                return cleanValueTo8Decimals.Normalize();
            }

            return value.Normalize();
        }

        public static IUnityContainer RegisterInstanceFunc<T1, T2>(this IUnityContainer unityContainer)
        {
            return unityContainer.RegisterInstance<Func<T1, T2>>(t => unityContainer.Resolve<T2>(new DependencyOverride<T1>(t)));
        }
    }
}