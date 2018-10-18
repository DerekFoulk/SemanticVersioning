using System;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global

namespace SemanticVersioning.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string s, string value, StringComparison comparisonType)
        {
            return s.IndexOf(value, comparisonType) >= 0;
        }

        public static string After(this string s, char value)
        {
            var arr = s.Split(new[] {value}, StringSplitOptions.None);

            return arr.Length >= 2 ? arr.LastOrDefault() : null;
        }

        public static string After(this string s, string value)
        {
            var arr = s.Split(new[] {value}, StringSplitOptions.None);

            return arr.Length >= 2 ? arr.LastOrDefault() : null;
        }

        public static string Before(this string s, char value)
        {
            var arr = s.Split(new[] {value}, StringSplitOptions.None);

            return arr.FirstOrDefault();
        }

        public static string Before(this string s, string value)
        {
            var arr = s.Split(new[] {value}, StringSplitOptions.None);

            return arr.FirstOrDefault();
        }

        public static bool IsNumber(this string s)
        {
            return !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s, @"^\d+$");
        }
    }
}
