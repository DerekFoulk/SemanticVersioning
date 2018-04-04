using PropertyChanged;
using System.Text.RegularExpressions;

namespace SemanticVersioning
{
    [AddINotifyPropertyChangedInterface]
    public class Version
    {
        public Version()
        {
        }

        public Version(string version)
        {
            if (TryParse(version, out Version result))
            {
                Major = result.Major;
                Minor = result.Minor;
                Patch = result.Patch;
                Build = result.Build;
            }
        }

        public int? Major { get; set; }
        public int? Minor { get; set; }
        public int? Patch { get; set; }
        public int? Build { get; set; }

        public bool TryParse(string s, out Version result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            var input = s.Trim();

            if (!Regex.IsMatch(input, @"^(\d+|\d+(\.\d+)+)$"))
                return false;

            var versions = input.Split('.');

            result = new Version
            {
                Major = versions.Length >= 1 ? int.Parse(versions[0]) : default(int?),
                Minor = versions.Length >= 2 ? int.Parse(versions[1]) : default(int?),
                Patch = versions.Length >= 3 ? int.Parse(versions[2]) : default(int?),
                Build = versions.Length >= 4 ? int.Parse(versions[3]) : default(int?)
            };

            return true;
        }

        public override string ToString()
        {
            string version = null;

            if (Major != null)
                version += Major;

            if (Minor != null)
                version += $".{Minor}";

            if (Patch != null)
                version += $".{Patch}";

            if (Build != null)
                version += $".{Build}";

            return version;
        }

        public string ToSemanticVersionString()
        {
            string version = null;

            version = $"{Major?.ToString() ?? "0"}.{Minor?.ToString() ?? "0"}.{Patch?.ToString() ?? "0"}";

            return version;
        }

        public string ToAssemblyVersionString()
        {
            string version = null;

            version = $"{Major?.ToString() ?? "0"}.{Minor?.ToString() ?? "0"}.{Patch?.ToString() ?? "0"}.{Build?.ToString() ?? "0"}";

            return version;
        }
    }
}
