using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Version CurrentVersion => GetHighestVersion();

        private Version GetHighestVersion()
        {
            var projects = _dte.Solution?.Projects;

            if (projects == null)
                return null;

            var versions = new List<string>();

            foreach (var item in projects)
            {
                try
                {
                    var project = (Project)item;
                    var version = project.Properties.Item("Version").Value.ToString();

                    versions.Add(version);
                }
                catch
                {
                }
            }

            var highestVersion = versions.Distinct().OrderByDescending(x => x).FirstOrDefault();

            return new Version(highestVersion);
        }

        public static void Initialize(DTE dte)
        {
            Instance = new SemanticVersioningManager(dte);
        }
    }
}
