using PropertyChanged;
using SemanticVersioning.Models;
using SemanticVersioning.Services;

namespace SemanticVersioning.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class VersionManagerToolWindowViewModel
    {
        private readonly VersionService _versionService;

        public VersionManagerToolWindowViewModel()
        {
            _versionService = new VersionService();
        }

        public Version Version { get; set; }

        public void Load()
        {
            Version = _versionService.GetHighestVersion();
        }

        public void Update()
        {
            _versionService.SetVersions(Version);
        }
    }
}