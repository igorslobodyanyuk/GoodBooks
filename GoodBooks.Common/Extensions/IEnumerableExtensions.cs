using System;
using System.Collections.Generic;
using System.Linq;

namespace GoodBooks.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> items) =>
            items ?? Array.Empty<T>();

        public static bool IsEmpty<T>(this IEnumerable<T> items) =>
            items == null || !items.Any();
    }
}