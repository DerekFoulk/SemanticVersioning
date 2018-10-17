using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using SemanticVersioning.Extensions;
using Project = SemanticVersioning.Models.Project;
using Version = SemanticVersioning.Models.Version;

namespace SemanticVersioning.Services
{
    internal sealed class VersionService
    {
        private readonly DTE _dte;

        private IEnumerable<Project> _projects;

        internal VersionService()
        {
            _dte = DteService.Instance.Dte;
        }

        private void LoadProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projects = new List<Project>();

            foreach (EnvDTE.Project project in _dte.Solution.Projects)
                try
                {
                    projects.Add(new Project(project));
                }
                catch
                {
                    // ignored
                }

            _projects = projects;
        }

        internal Version GetHighestVersion()
        {
            LoadProjects();

            // Get versions from files
            var versions = _projects
                .SelectMany(x => x.Files
                    .Where(y => !y.Versions.IsNullOrEmpty())
                    .SelectMany(y => y.Versions)
                );

            // Get unique versions
            var uniqueVersions = versions
                .GroupBy(x => new
                {
                    x.Major,
                    x.Minor,
                    x.Patch,
                    x.Build,
                    x.Suffix
                })
                .Select(x => x.FirstOrDefault());

            // Get highest version (ignoring zeros during sorting)
            var sortedVersions = uniqueVersions
                .OrderByDescending(x => x?.Major == 0 ? null : x?.Major)
                .ThenByDescending(x => x?.Minor == 0 ? null : x?.Minor)
                .ThenByDescending(x => x?.Patch == 0 ? null : x?.Patch)
                .ThenByDescending(x => x?.Build == 0 ? null : x?.Build)
                .ThenByDescending(x => x.Suffix);

            var highestVersion = sortedVersions.FirstOrDefault();

            return highestVersion ?? new Version("1.0.0");
        }

        internal void SetVersions(Version version)
        {
            if (version.IsNullOrEmpty())
                throw new ArgumentException();

            LoadProjects();

            foreach (var project in _projects)
            foreach (var file in project.Files)
                file.SetVersions(version);
        }
    }
}
