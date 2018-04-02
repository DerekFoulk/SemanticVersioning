using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace SemanticVersioning
{
    public static class VersionManager
    {
        public static DTE DTE { get; set; }

        private static string GetVersionFromProject(Project project)
        {
            var version = project.Properties.Item("Version").Value;
            //var fileVersion = project.Properties.Item("FileVersion").Value;
            //var assemblyVersion = project.Properties.Item("AssemblyVersion").Value;

            return version.ToString();
        }

        public static Version GetCurrentVersion()
        {
            var solution = DTE.Solution;

            var projects = solution.Projects;

            var versions = new List<string>();

            foreach (var project in projects)
            {
                try
                {
                    versions.Add(GetVersionFromProject((Project)project));
                }
                catch
                {
                }
            }

            var version = versions.Distinct().OrderByDescending(x => x).FirstOrDefault();

            return new Version(version);
        }

        public static void SetVersion()
        {
            //DTE dte = (DTE)GetGlobalService(typeof(DTE));
            //Project project = dte.Solution.Projects.Item(1);

            //string uniqueName = project.UniqueName;
            //IVsSolution solution = (IVsSolution)GetGlobalService(typeof(SVsSolution));
            //IVsHierarchy hierarchy;
            //solution.GetProjectOfUniqueName(uniqueName, out hierarchy);
            //IVsBuildPropertyStorage buildPropertyStorage = hierarchy as IVsBuildPropertyStorage;

            //if (buildPropertyStorage != null)
            //{
            //    uint itemId;
            //    string fullPath = (string)project.ProjectItems.Item("Class1.cs").Properties.Item("FullPath").Value;
            //    hierarchy.ParseCanonicalName(fullPath, out itemId);
            //    buildPropertyStorage.SetItemAttribute(
            //        itemId, "MyAttribute", "MyValue");
            //}
        }
    }
}
