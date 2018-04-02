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

        public Version(string version)
        {
            var arrVersion = version.Split('.');

            if (arrVersion.Length >= 1)
                Major = int.Parse(arrVersion[0]);

            if (arrVersion.Length >= 2)
                Minor = int.Parse(arrVersion[1]);

            if (arrVersion.Length >= 3)
                Patch = int.Parse(arrVersion[2]);

            if (arrVersion.Length >= 4)
                Build = int.Parse(arrVersion[3]);
        }

        public int Major { get; set; } = 1;
        public int Minor { get; set; } = 0;
        public int Patch { get; set; } = 0;
        public int Build { get; set; } = 0;
    }
}
