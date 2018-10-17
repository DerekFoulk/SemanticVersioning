using System.Text.RegularExpressions;
using SemanticVersioning.Extensions;

namespace SemanticVersioning.Models
{
    public class Version
    {
        private Version()
        {
        }

        public Version(string version)
        {
            if (!TryParse(version, out var result))
                return;

            Major = result.Major;
            Minor = result.Minor;
            Patch = result.Patch;
            Build = result.Build;
            Suffix = result.Suffix;
        }

        public int Major { get; set; }

        public int? Minor { get; set; }

        public int? Patch { get; set; }

        public int? Build { get; set; }

        public string Suffix { get; set; }

        private static bool TryParse(string s, out Version result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            var input = s.Trim();

            if (!Regex.IsMatch(input, @"^\d+(\.\d+){0,3}(-\w+(\.\w+)*)*$"))
                return false;

            var versions = input.Before('-').Split('.');

            result = new Version
            {
                Major = versions.Length >= 1 ? int.Parse(versions[0]) : default(int),
                Minor = versions.Length >= 2 ? int.Parse(versions[1]) : default(int?),
                Patch = versions.Length >= 3 ? int.Parse(versions[2]) : default(int?),
                Build = versions.Length >= 4 ? int.Parse(versions[3]) : default(int?)
            };

            var suffix = input.After('-');

            if (!string.IsNullOrWhiteSpace(suffix))
                result.Suffix = suffix;

            return true;
        }

        public override string ToString()
        {
            var version = $"{Major}";

            if (Minor != null)
            {
                version += $".{Minor}";

                if (Patch != null)
                {
                    version += $".{Patch}";

                    if (Build != null)
                        version += $".{Build}";
                }
            }

            if (!string.IsNullOrWhiteSpace(Suffix))
                version += $"-{Suffix}";

            return version;
        }

        public string ToAssemblyVersionString()
        {
            return $"{Major}.{Minor ?? 0}.{Patch ?? 0}.{Build ?? 0}";
        }
    }
}
