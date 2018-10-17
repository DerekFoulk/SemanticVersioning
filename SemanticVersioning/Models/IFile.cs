using System.Collections.Generic;
// ReSharper disable UnusedMemberInSuper.Global

namespace SemanticVersioning.Models
{
    public interface IFile
    {
        string FileName { get; set; }

        IEnumerable<Version> Versions { get; set; }

        IEnumerable<Version> GetVersions();

        void SetVersions(Version version);
    }
}
