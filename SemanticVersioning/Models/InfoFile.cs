using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SemanticVersioning.Models
{
    public class InfoFile : IFile
    {
        public InfoFile(string fileName)
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

            var keyNode = xDocument?.Element("plist")?.Element("dict")?.Descendants("key")?.FirstOrDefault(x => x.Value == "CFBundleShortVersionString");

            if (keyNode?.NextNode is XElement valueNode && valueNode.Name == "string")
            {
                var version = valueNode.Value;
                versions.Add(new Version(version));
            }

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);

            xDocument.DocumentType.InternalSubset = null;

            var dict = xDocument?.Element("plist")?.Element("dict");

            AddOrUpdate("CFBundleShortVersionString", version.ToString(), dict);

            xDocument?.Save(FileName);
        }

        private void AddOrUpdate(string key, string value, XElement dict)
        {
            var keyElement = dict?.Descendants("key")?.FirstOrDefault(x => x.Value == key);

            if (keyElement == default(XElement))
            {
                keyElement = new XElement("key")
                {
                    Value = key
                };

                var valueElement = new XElement("string")
                {
                    Value = value
                };

                dict?.Add(keyElement);
                dict?.Add(valueElement);
            }
            else
            {
                if (keyElement?.NextNode is XElement valueElement && valueElement.Name == "string")
                    valueElement.Value = value;
            }
        }
    }
}
