using System;
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

            var keyNode = xDocument?.Element("plist")?.Element("dict")?.Descendants("key")
                ?.FirstOrDefault(x => x.Value == "CFBundleShortVersionString");
            RunIfKvpExists(keyNode, "string", valueNode =>
            {
                var version = valueNode.Value;
                versions.Add(new Version(version));
            });

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            var xDocument = XDocument.Load(FileName);

            xDocument.DocumentType.InternalSubset = null;

            var dict = xDocument?.Element("plist")?.Element("dict");

            AddOrUpdate("CFBundleShortVersionString", version.ToString(), dict);

            var bundleVersion = default(int);
            var bundleVersionNode = dict?.Descendants("key")?.FirstOrDefault(x => x.Value == "CFBundleVersion");

            RunIfKvpExists(bundleVersionNode, "string",
                bundleVersionValueNode =>
                {
                    bundleVersion = decimal.TryParse(bundleVersionValueNode.Value, out var result) ? (int) result : 0;
                });

            AddOrUpdate("CFBundleVersion", $"{++bundleVersion}", dict);

            xDocument?.Save(FileName);
        }

        private void RunIfKvpExists(XElement keyNode, string valueNodeName, Action<XElement> action)
        {
            if (keyNode?.NextNode is XElement valueNode && valueNode.Name == valueNodeName)
                action.Invoke(valueNode);
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
                RunIfKvpExists(keyElement, "string", valueNode => { valueNode.Value = value; });
            }
        }
    }
}