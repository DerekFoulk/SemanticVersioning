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
                var value = xDocument?.Element("Project")?.Element("PropertyGroup")?.Element(element)?.Value;

                if (string.IsNullOrEmpty(value))
                    continue;

                var version = new Version(value);
                versions.Add(version);
            }

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);

            xDocument?.Element("Project")?.Element("PropertyGroup")?.SetElementValue("Version", version.ToString());
            xDocument?.Element("Project")?.Element("PropertyGroup")?.SetElementValue("AssemblyVersion", null);
            xDocument?.Element("Project")?.Element("PropertyGroup")?.SetElementValue("FileVersion", null);

            var xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };

            using (var xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings))
            {
                xDocument?.Save(xmlWriter);
            }
        }
    }
}
