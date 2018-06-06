using System;

namespace SemanticVersioning.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string s, string value, StringComparison comparisonType)
        {
            return s.IndexOf(value, comparisonType) >= 0;
        }
    }
}
