using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SemanticVersioning.Extensions;

namespace SemanticVersioning.Models
{
    public class Project
    {
        private readonly EnvDTE.Project _project;

        public Project(EnvDTE.Project project)
        {
            if (string.IsNullOrWhiteSpace(project.FileName))
                throw new ArgumentException("Project does not exist.", "project");

            _project = project;

            FileName = project.FileName;

            Type = GetProjectType();

            Files = GetFiles();
        }

        public string FileName { get; set; }

        public ProjectType Type { get; set; }

        public IEnumerable<IFile> Files { get; set; }

        private ProjectType GetProjectType()
        {
            var projectType = default(ProjectType);

            var xDocument = XDocument.Load(FileName);

            var targetFramework = xDocument?
                .Element("Project")?
                .Element("PropertyGroup")?
                .Element("TargetFramework")?.Value;

            if (!string.IsNullOrWhiteSpace(targetFramework))
            {
                projectType = ProjectTypeIds.TargetFrameworks.FirstOrDefault(x =>
                    x.Value.Any(y => targetFramework.Contains(y, StringComparison.OrdinalIgnoreCase))).Key;
            }
            else
            {
                XNamespace xNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

                var projectTypeGuids = xDocument?
                    .Elements(xNamespace + "Project")?
                    .Elements(xNamespace + "PropertyGroup")?
                    .Elements(xNamespace + "ProjectTypeGuids")?
                    .Select(x => x.Value)
                    .SelectMany(x => x.Replace("{", string.Empty).Replace("}", string.Empty).Split(';'));

                projectType = projectTypeGuids
                    .Select(x => ProjectTypeIds.Guids.FirstOrDefault(y => y.Value.Contains(x)).Key).FirstOrDefault();
            }

            return projectType;
        }

        private void TryAddFiles(List<IFile> files, string projectDirectory, Type type, string searchPattern = "*")
        {
            var targetFiles = Directory.GetFiles(projectDirectory, searchPattern, SearchOption.AllDirectories)
                .Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\"));
            foreach (var targetFile in targetFiles)
                if (type == typeof(ProjectFile))
                    files.Add(new ProjectFile(targetFile));
                else if (type == typeof(AssemblyInfoFile))
                    files.Add(new AssemblyInfoFile(targetFile));
                else if (type == typeof(AndroidManifestFile))
                    files.Add(new AndroidManifestFile(targetFile));
                else if (type == typeof(InfoFile))
                    files.Add(new InfoFile(targetFile));
                else if (type == typeof(PackageFile))
                    files.Add(new PackageFile(targetFile));
        }

        private IEnumerable<IFile> GetFiles()
        {
            var files = new List<IFile>();

            var projectDirectory = Path.GetDirectoryName(FileName);

            switch (Type)
            {
                case ProjectType.NetCore:
                    TryAddFiles(files, projectDirectory, typeof(ProjectFile), "*.csproj");
                    break;

                case ProjectType.NetStandard:
                    TryAddFiles(files, projectDirectory, typeof(ProjectFile), "*.csproj");
                    break;

                case ProjectType.XamarinAndroid:
                    TryAddFiles(files, projectDirectory, typeof(AssemblyInfoFile), "*AssemblyInfo.cs");
                    TryAddFiles(files, projectDirectory, typeof(AndroidManifestFile), "*AndroidManifest.xml");
                    break;

                case ProjectType.XamarinIos:
                    TryAddFiles(files, projectDirectory, typeof(AssemblyInfoFile), "*AssemblyInfo.cs");
                    TryAddFiles(files, projectDirectory, typeof(InfoFile), "*Info.plist");
                    break;

                case ProjectType.Uwp:
                    TryAddFiles(files, projectDirectory, typeof(AssemblyInfoFile), "*AssemblyInfo.cs");
                    TryAddFiles(files, projectDirectory, typeof(PackageFile), "*Package.appxmanifest");
                    break;

                default:
                    TryAddFiles(files, projectDirectory, typeof(AssemblyInfoFile), "*AssemblyInfo.cs");
                    break;
            }

            return files;
        }
    }
}
