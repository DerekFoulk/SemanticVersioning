namespace SemanticVersioning
{
    public class Version
    {
        public Version()
        {
        }

        public Version(int major, int minor, int patch, int build = 0)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        public int Major { get; set; } = 1;
        public int Minor { get; set; } = 0;
        public int Patch { get; set; } = 0;
        public int Build { get; set; } = 0;
    }
}
