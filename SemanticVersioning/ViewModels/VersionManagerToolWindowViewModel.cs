using SemanticVersioning.Models;

namespace SemanticVersioning.ViewModels
{
    public class VersionManagerToolWindowViewModel
    {
        public VersionManagerToolWindowViewModel()
        {
            Version = new Version("1.0.0");
        }

        public Version Version { get; set; }
    }
}
