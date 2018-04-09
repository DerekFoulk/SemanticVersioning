using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            var versions = new List<Version>();

            foreach (Project project in projects)
            {
                if (string.IsNullOrWhiteSpace(project.FileName))
                    continue;

                try
                {
                    var version = project.Properties.Item("Version").Value.ToString();
                    versions.Add(new Version(version));
                }
                catch
                {
                }

                try
                {
                    var version = project.Properties.Item("AssemblyVersion").Value.ToString();
                    versions.Add(new Version(version));
                }
                catch
                {
                }

                try
                {
                    var version = project.Properties.Item("FileVersion").Value.ToString();
                    versions.Add(new Version(version));
                }
                catch
                {
                }

                var assemblyInfoFiles = GetAssemblyInfoFiles(project);

                if (assemblyInfoFiles == default(IEnumerable<string>) || !assemblyInfoFiles.Any())
                    continue;

                foreach (var assemblyInfoFile in assemblyInfoFiles)
                {
                    try
                    {
                        var file = File.ReadAllText(assemblyInfoFile);
                        var matches = Regex.Matches(file, RegexPatterns.AssemblyInfoVersions);

                        foreach (Match match in matches)
                        {
                            var versionInMatch = Regex.Match(match.Value, RegexPatterns.VersionNumbers).Value;
                            versions.Add(new Version(versionInMatch));
                        }
                    }
                    catch
                    {
                    }
                }
            }

            var highestVersion = versions.OrderByDescending(x => x.ToAssemblyVersionString()).FirstOrDefault();

            return highestVersion;
        }

        public void SetVersion(Version version)
        {
            var projects = _dte.Solution?.Projects;

            if (projects == default(Projects))
                return;

            foreach (Project project in projects)
            {
                if (string.IsNullOrWhiteSpace(project.FileName))
                    continue;

                try
                {
                    SetProjectVersion(project, version);
                }
                catch
                {
                }

                try
                {
                    SetAssemblyInfoVersion(project, version);
                }
                catch
                {
                }
            }
        }

        private void SetProjectVersion(Project project, Version version)
        {
            try
            {
                var projectXml = new XmlDocument();
                projectXml.Load(project.FileName);

                var versionNode = projectXml.SelectSingleNode("Project/PropertyGroup/Version");

                if (versionNode == default(XmlNode))
                {
                    var assemblyInfoFiles = GetAssemblyInfoFiles(project);

                    if (assemblyInfoFiles == default(IEnumerable<string>) || !assemblyInfoFiles.Any())
                        versionNode = projectXml.SelectSingleNode("Project/PropertyGroup").AppendChild(projectXml.CreateNode(XmlNodeType.Element, "Version", null));
                }

                versionNode.InnerText = version.ToString();

                var assemblyVersionNode = projectXml.SelectSingleNode("Project/PropertyGroup/AssemblyVersion");

                if (assemblyVersionNode != default(XmlNode))
                    assemblyVersionNode.ParentNode.RemoveChild(assemblyVersionNode);

                var fileVersionNode = projectXml.SelectSingleNode("Project/PropertyGroup/FileVersion");

                if (fileVersionNode != default(XmlNode))
                    fileVersionNode.ParentNode.RemoveChild(fileVersionNode);

                projectXml.Save(project.FileName);
            }
            catch
            {
            }
        }

        private void SetAssemblyInfoVersion(Project project, Version version)
        {
            var assemblyInfoFiles = GetAssemblyInfoFiles(project);

            if (assemblyInfoFiles == default(IEnumerable<string>) || !assemblyInfoFiles.Any())
                return;

            foreach (var assemblyInfoFile in assemblyInfoFiles)
            {
                try
                {
                    File.Move(assemblyInfoFile, $"{assemblyInfoFile}.bak");

                    try
                    {
                        var lines = File.ReadLines($"{assemblyInfoFile}.bak");

                        using (var file = new StreamWriter(assemblyInfoFile, true, new UTF8Encoding(true)))
                        {
                            foreach (var line in lines)
                            {
                                if (line.Contains("AssemblyVersion"))
                                {
                                    var updatedLine = Regex.Replace(line, RegexPatterns.AssemblyVersion, $"AssemblyVersion(\"{version.ToAssemblyVersionString()}\")");

                                    file.WriteLine(updatedLine);
                                }
                                else if (line.Contains("AssemblyFileVersion"))
                                {
                                    var updatedLine = Regex.Replace(line, RegexPatterns.AssemblyFileVersion, $"AssemblyFileVersion(\"{version.ToAssemblyVersionString()}\")");

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

        private IEnumerable<string> GetAssemblyInfoFiles(Project project)
        {
            IEnumerable<string> assemblyInfoFiles = default(IEnumerable<string>);

            try
            {
                var projectDirectory = Path.GetDirectoryName(project.FileName);
                var projectFiles = Directory.GetFiles(projectDirectory, "*", SearchOption.AllDirectories);
                assemblyInfoFiles = projectFiles.Where(x => !x.Contains("bin") && !x.Contains("obj")).Where(x => x.EndsWith("AssemblyInfo.cs"));
            }
            catch
            {
            }

            return assemblyInfoFiles;
        }
    }
}
