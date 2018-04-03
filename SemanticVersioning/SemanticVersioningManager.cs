using EnvDTE;
using System;

namespace SemanticVersioning
{
    internal sealed class SemanticVersioningManager
    {
        private DTE _dte;

        private SemanticVersioningManager(DTE dte)
        {
            _dte = dte ?? throw new ArgumentNullException("dte");
        }

        public static SemanticVersioningManager Instance { get; private set; }

        public static void Initialize(DTE dte)
        {
            Instance = new SemanticVersioningManager(dte);
        }
    }
}
