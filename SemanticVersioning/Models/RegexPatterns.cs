namespace SemanticVersioning.Models
{
    public static class RegexPatterns
    {
        public static string VersionNumbers => @"\d+(\.\d+)*";

        public static string AssemblyInfoVersions => $@"Assembly(File)*Version\(""{VersionNumbers}""\)";

        public static string AssemblyVersion => $@"AssemblyVersion\(""{VersionNumbers}""\)";

        public static string AssemblyFileVersion => $@"AssemblyFileVersion\(""{VersionNumbers}""\)";
    }
}