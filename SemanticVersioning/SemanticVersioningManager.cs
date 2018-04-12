using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

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

        public Version Version
        {
            get
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

                    try
                    {
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
                    catch
                    {
                    }

                    try
                    {
                        var androidManifestFiles = GetAndroidManifestFiles(project);

                        foreach (string androidManifestFile in androidManifestFiles)
                        {
                            try
                            {
                                var xDocument = XDocument.Load(androidManifestFile);
                                XNamespace xNamespace = "http://schemas.android.com/apk/res/android";

                                var manifests = xDocument?.Elements("manifest");

                                foreach (XElement manifest in manifests)
                                {
                                    var version = manifest?.Attribute(xNamespace + "versionName")?.Value;
                                    versions.Add(new Version(version));
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    try
                    {
                        var infoFiles = GetInfoFiles(project);

                        foreach (string infoFile in infoFiles)
                        {
                            try
                            {
                                var xDocument = XDocument.Load(infoFile);

                                var keyNode = xDocument?.Element("plist")?.Element("dict")?.Descendants("key")?.FirstOrDefault(x => x.Value == "CFBundleShortVersionString");

                                if (keyNode?.NextNode is XElement valueNode && valueNode.Name == "string")
                                {
                                    var version = valueNode.Value;
                                    versions.Add(new Version(version));
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

                var highestVersion = versions.OrderByDescending(x => x.ToAssemblyVersionString()).FirstOrDefault();

                return highestVersion;
            }
            set
            {
                var version = value;

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

                    try
                    {
                        SetMobileVersion(project, version);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void SetProjectVersion(Project project, Version version)
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

        private void SetMobileVersion(Project project, Version version)
        {
            // Check if the project is either a Xamarin.Android or a Xamarin.iOS project
            var projectXml = new XmlDocument();
            projectXml.Load(project.FileName);

            if (projectXml == default(XmlDocument))
                return;

            var xmlNamespaceManager = new XmlNamespaceManager(projectXml.NameTable);
            xmlNamespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

            var projectTypeGuids = projectXml.SelectSingleNode("//msbuild:ProjectTypeGuids", xmlNamespaceManager).InnerText;

            if (projectTypeGuids == null)
                return;

            var isXamarinAndroid = projectTypeGuids.Contains("{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}");
            var isXamarinIos = projectTypeGuids.Contains("{FEACFBD2-3405-455C-9665-78FE426C6842}");

            if (isXamarinAndroid)
            {
                SetXamarinAndroidVersion(project, version);
            }
            else if (isXamarinIos)
            {
                SetXamarinIosVersion(project, version);
            }
        }

        private void SetXamarinAndroidVersion(Project project, Version version)
        {
            var androidManifestFiles = GetAndroidManifestFiles(project);

            foreach (string androidManifestFile in androidManifestFiles)
            {
                try
                {
                    var xDocument = XDocument.Load(androidManifestFile);
                    XNamespace xNamespace = "http://schemas.android.com/apk/res/android";
                    var manifest = xDocument?.Element("manifest");

                    var versionCode = int.TryParse(manifest?.Attribute(xNamespace + "versionCode")?.Value, out int tmp) ? tmp : 0;
                    versionCode++;

                    manifest?.SetAttributeValue(xNamespace + "versionCode", versionCode.ToString());
                    manifest?.SetAttributeValue(xNamespace + "versionName", version.ToString());

                    xDocument.Save(androidManifestFile);
                }
                catch
                {
                }
            }
        }

        private void SetXamarinIosVersion(Project project, Version version)
        {
            var infoFiles = GetInfoFiles(project);

            foreach (string infoFile in infoFiles)
            {
                try
                {
                    var xDocument = XDocument.Load(infoFile);

                    xDocument.DocumentType.InternalSubset = null;

                    var dict = xDocument?.Element("plist")?.Element("dict");

                    var bundleVersion = decimal.TryParse((dict?.Descendants("key")?.FirstOrDefault(x => x.Value == "CFBundleVersion")?.NextNode as XElement)?.Value, out decimal tmp) ? (int)tmp : 0;
                    bundleVersion++;

                    PlistAddOrUpdate("CFBundleVersion", bundleVersion.ToString(), dict);

                    PlistAddOrUpdate("CFBundleShortVersionString", version.ToString(), dict);

                    xDocument.Save(infoFile);
                }
                catch
                {
                }
            }
        }

        private void PlistAddOrUpdate(string key, string value, XElement dict)
        {
            var currentKey = dict?.Descendants("key")?.FirstOrDefault(x => x.Value == key);

            if (currentKey == default(XElement))
            {
                var keyNode = new XElement("key")
                {
                    Value = key
                };

                var valueNode = new XElement("string")
                {
                    Value = value
                };

                dict?.Add(keyNode);
                dict?.Add(valueNode);
            }
            else
            {
                if (currentKey?.NextNode is XElement currentValue && currentValue.Name == "string")
                    currentValue.Value = value;
            }
        }

        private IEnumerable<string> GetAssemblyInfoFiles(Project project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FileName);
            var projectFiles = Directory.GetFiles(projectDirectory, "*", SearchOption.AllDirectories);
            var assemblyInfoFiles = projectFiles.Where(x => !x.Contains("bin") && !x.Contains("obj")).Where(x => x.EndsWith("AssemblyInfo.cs"));

            return assemblyInfoFiles;
        }

        private IEnumerable<string> GetAndroidManifestFiles(Project project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FileName);
            var androidManifestFiles = Directory.GetFiles(projectDirectory, "*AndroidManifest.xml", SearchOption.AllDirectories).Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\"));

            return androidManifestFiles;
        }

        private IEnumerable<string> GetInfoFiles(Project project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FileName);
            var infoFiles = Directory.GetFiles(projectDirectory, "*Info.plist", SearchOption.AllDirectories).Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\"));

            return infoFiles;
        }

        public static void Initialize(DTE dte)
        {
            Instance = new SemanticVersioningManager(dte);
        }
    }
}
