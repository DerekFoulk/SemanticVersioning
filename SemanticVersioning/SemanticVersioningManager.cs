using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

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

        public Version CurrentVersion => GetVersion();

        private Version GetVersion()
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

        // Set Project Properties
        public void SetVersion(Version version)
        {
            var projects = _dte.Solution?.Projects;

            if (projects == null)
                return;

            foreach (var p in projects)
            {
                var project = (Project)p;

                if (string.IsNullOrWhiteSpace(project.FileName))
                    continue;

                // .NET Standard Projects
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(project.FileName);

                    var xmlNode = xmlDoc.SelectSingleNode("Project/PropertyGroup/Version");
                    xmlNode.InnerText = version.ToString();

                    xmlDoc.Save(project.FileName);
                }
                catch
                {
                }

                // AssemblyInfo Files
                try
                {
                    var projectDirectory = Path.GetDirectoryName(project.FileName);
                    var projectFiles = Directory.GetFiles(projectDirectory, "*", SearchOption.AllDirectories);
                    var assemblyInfoFiles = projectFiles.Where(x => !x.Contains("bin") && !x.Contains("obj")).Where(x => x.EndsWith("AssemblyInfo.cs"));

                    foreach (var assemblyInfoFile in assemblyInfoFiles)
                    {
                        // Rename source file as .bak
                        File.Move(assemblyInfoFile, $"{assemblyInfoFile}.bak");

                        // Create new file with updates in its place
                        var lines = File.ReadLines($"{assemblyInfoFile}.bak");

                        using (var file = new StreamWriter(assemblyInfoFile))
                        {
                            foreach (var line in lines)
                            {
                                if (line.Contains("AssemblyVersion"))
                                {
                                    var pattern = "AssemblyVersion\\(\"(\\d+|\\d+(\\.\\d+)+)\\.*\\**\"\\)";

                                    var updatedLine = Regex.Replace(line, pattern, $"AssemblyVersion(\"{version.ToAssemblyVersionString()}\")");

                                    file.WriteLine(updatedLine);
                                }
                                else if (line.Contains("AssemblyFileVersion"))
                                {
                                    var pattern = "AssemblyFileVersion\\(\"(\\d+|\\d+(\\.\\d+)+)\\.*\\**\"\\)";

                                    var updatedLine = Regex.Replace(line, pattern, $"AssemblyFileVersion(\"{version.ToAssemblyVersionString()}\")");

                                    file.WriteLine(updatedLine);
                                }
                                else
                                {
                                    file.WriteLine(line);
                                }
                            }

                            file.Close();
                        }

                        File.Delete($"{assemblyInfoFile}.bak");
                    }
                }
                catch
                {
                }
            }
        }
    }
}
