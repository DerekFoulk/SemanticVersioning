using EnvDTE;
using SemanticVersioning.Models;

namespace SemanticVersioning.Services
{
    internal sealed class VersionService
    {
        private readonly DTE _dte;

        internal VersionService()
        {
            _dte = DteService.Instance.DTE;
        }

        internal Version GetHighestVersion()
        {
            return new Version("1.0.0");
        }
    }
}
