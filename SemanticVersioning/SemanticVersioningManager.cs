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

        public static void Initialize(DTE dte)
        {
            Instance = new SemanticVersioningManager(dte);
        }

        private Version GetVersion()
        {
            var projects = _dte.Solution?.Projects;

            if (projects == default(Projects))
                return null;

            var versions = new List<string>();

            foreach (var item in projects)
            {
                try
                {
                    var project = (Project)item;

                    if (string.IsNullOrWhiteSpace(project.FileName))
                        continue;

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

        // Set Project Properties
        public void SetVersion(Version version)
        {
            var projects = _dte.Solution?.Projects;

            if (projects == default(Projects))
                return;

            foreach (var item in projects)
            {
                try
                {
                    var project = (Project)item;

                    if (string.IsNullOrWhiteSpace(project.FileName))
                        continue;

                    // Project File
                    SetProjectVersion(project, version);

                    // AssemblyInfo Files
                    SetAssemblyInfoVersion(project, version);
                }
                catch
                {
                }
            }
        }

        private void SetProjectVersion(Project project, Version version)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(project.FileName);

            var xmlNode = xmlDoc.SelectSingleNode("Project/PropertyGroup/Version");
            xmlNode.InnerText = version.ToString();

            xmlDoc.Save(project.FileName);
        }

        private void SetAssemblyInfoVersion(Project project, Version version)
        {
            var projectDirectory = Path.GetDirectoryName(project.FileName);
            var projectFiles = Directory.GetFiles(projectDirectory, "*", SearchOption.AllDirectories);
            var assemblyInfoFiles = projectFiles?.Where(x => !x.Contains("bin") && !x.Contains("obj")).Where(x => x.EndsWith("AssemblyInfo.cs"));

            if (assemblyInfoFiles == default(IEnumerable<string>))
                return;

            foreach (var assemblyInfoFile in assemblyInfoFiles)
            {
                try
                {
                    File.Move(assemblyInfoFile, $"{assemblyInfoFile}.bak");

                    try
                    {
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
                    }
                    catch
                    {
                        if (File.Exists(assemblyInfoFile))
                            File.Delete(assemblyInfoFile);

                        if (File.Exists($"{assemblyInfoFile}.bak"))
                            File.Move($"{assemblyInfoFile}.bak", assemblyInfoFile);
                    }
                    finally
                    {
                        if (File.Exists($"{assemblyInfoFile}.bak"))
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
