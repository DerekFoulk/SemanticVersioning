using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SemanticVersioning.Models
{
    public class PackageFile : IFile
    {
        public PackageFile(string fileName)
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
            XNamespace xNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

            var version = xDocument?.Element(xNamespace + "Package")?.Element(xNamespace + "Identity")
                ?.Attribute("Version")?.Value;
            versions.Add(new Version(version));

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);
            XNamespace xNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

            xDocument?.Element(xNamespace + "Package")?.Element(xNamespace + "Identity")
                ?.SetAttributeValue("Version", version.ToAssemblyVersionString());

            var xmlWriterSettings = new XmlWriterSettings
            {
                NewLineOnAttributes = true,
                Indent = true
            };

            using (var xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings))
            {
                xDocument?.Save(xmlWriter);
            }
        }
    }
}
