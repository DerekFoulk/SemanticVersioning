using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SemanticVersioning.Models
{
    public class ProjectFile : IFile
    {
        public ProjectFile(string fileName)
        {
            FileName = fileName;

            Versions = GetVersions();
        }

        public string FileName { get; set; }

        public IEnumerable<Version> Versions { get; set; }

        public IEnumerable<Version> GetVersions()
        {
            var versions = new List<Version>();

            var xDocument = XDocument.Load(FileName);

            var elements = new List<string>
            {
                "Version",
                "AssemblyVersion",
                "FileVersion"
            };

            foreach (var element in elements)
            {
                var version = xDocument.Element("Project")?.Element("PropertyGroup")?.Element(element)?.Value;

                try
                {
                    versions.Add(new Version(version));
                }
                catch
                {
                    // ignored
                }
            }

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);

            xDocument.Element("Project")?.Element("PropertyGroup")
                ?.SetElementValue("Version", version.ToVersionString());

            var build = version.Build;
            xDocument.Element("Project")?.Element("PropertyGroup")?.SetElementValue("AssemblyVersion",
                !string.IsNullOrWhiteSpace(build) && build.Equals("*", StringComparison.Ordinal)
                    ? version.ToAssemblyVersionString()
                    : null);

            xDocument.Element("Project")?.Element("PropertyGroup")?.SetElementValue("FileVersion", null);

            var xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };

            using (var xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings))
            {
                xDocument.Save(xmlWriter);
            }
        }
    }
}
