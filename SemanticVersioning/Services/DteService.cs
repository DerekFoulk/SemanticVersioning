using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace SemanticVersioning.Services
{
    internal sealed class DteService
    {
        private DteService(Package package)
        {
            var serviceProvider = package as IServiceProvider ?? throw new ArgumentNullException();

            DTE = (DTE) serviceProvider.GetService(typeof(DTE));
        }

        internal static DteService Instance { get; private set; }

        internal DTE DTE { get; }

        internal static void Initialize(Package package)
        {
            Instance = new DteService(package);
        }
    }
}
