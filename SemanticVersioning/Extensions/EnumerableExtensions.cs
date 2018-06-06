using System.Collections.Generic;
using System.Linq;

namespace SemanticVersioning.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return !(items?.Any() ?? false);
        }
    }
}