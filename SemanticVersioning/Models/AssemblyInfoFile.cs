using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticVersioning.Models
{
    public class AssemblyInfoFile : IFile
    {
        public AssemblyInfoFile(string fileName)
        {
            FileName = fileName;

            Versions = GetVersions();
        }

        public string FileName { get; set; }

        public IEnumerable<Version> Versions { get; set; }

        public IEnumerable<Version> GetVersions()
        {
            var versions = new List<Version>();

            var file = File.ReadAllText(FileName);
            var matches = Regex.Matches(file, RegexPatterns.AssemblyInfoVersions);

            foreach (Match match in matches)
            {
                var version = Regex.Match(match.Value, RegexPatterns.VersionNumbers).Value;
                versions.Add(new Version(version));
            }

            return versions.Any() ? versions : null;
        }

        public void SetVersions(Version version)
        {
            try
            {
                File.Move(FileName, $"{FileName}.bak");

                var lines = File.ReadLines($"{FileName}.bak");

                using (var file = new StreamWriter(FileName, true, new UTF8Encoding(true)))
                {
                    foreach (var line in lines)
                        if (line.Contains("AssemblyVersion"))
                        {
                            var updatedLine = Regex.Replace(line, RegexPatterns.AssemblyVersion,
                                $"AssemblyVersion(\"{version.ToAssemblyVersionString()}\")");

                            file.WriteLine(updatedLine);
                        }
                        else if (line.Contains("AssemblyFileVersion"))
                        {
                            var updatedLine = Regex.Replace(line, RegexPatterns.AssemblyFileVersion,
                                $"AssemblyFileVersion(\"{version.ToAssemblyVersionString()}\")");

                            file.WriteLine(updatedLine);
                        }
                        else
                        {
                            file.WriteLine(line);
                        }

                    file.Close();
                }
            }
            catch
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);

                if (File.Exists($"{FileName}.bak"))
                    File.Move($"{FileName}.bak", FileName);

                throw;
            }
            finally
            {
                if (File.Exists($"{FileName}.bak"))
                    File.Delete($"{FileName}.bak");
            }
        }
    }
}
