using SemanticVersioning.Models;
using SemanticVersioning.Services;

namespace SemanticVersioning.ViewModels
{
    public class VersionManagerToolWindowViewModel
    {
        private readonly VersionService _versionService;

        public VersionManagerToolWindowViewModel()
        {
            _versionService = new VersionService();

            Version = _versionService.GetHighestVersion();
        }

        public Version Version { get; set; }
    }
}
