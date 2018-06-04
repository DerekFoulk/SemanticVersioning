using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SemanticVersioning.Models
{
    public class AndroidManifestFile : IFile
    {
        public AndroidManifestFile(string fileName)
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
            XNamespace xNamespace = "http://schemas.android.com/apk/res/android";

            var version = xDocument?.Element("manifest")?.Attribute(xNamespace + "versionName")?.Value;
            versions.Add(new Version(version));

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);
            XNamespace xNamespace = "http://schemas.android.com/apk/res/android";

            xDocument?.Element("manifest")?.SetAttributeValue(xNamespace + "versionName", version.ToString());

            xDocument?.Save(FileName);
        }
    }
}
