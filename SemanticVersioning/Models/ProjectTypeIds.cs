using System.Collections.Generic;

namespace SemanticVersioning.Models
{
    public static class ProjectTypeIds
    {
        public static readonly Dictionary<ProjectType, List<string>> TargetFrameworks =
            new Dictionary<ProjectType, List<string>>
            {
                {
                    ProjectType.NetCore,
                    new List<string>
                    {
                        "netcoreapp"
                    }
                },
                {
                    ProjectType.NetStandard,
                    new List<string>
                    {
                        "netstandard"
                    }
                }
            };

        public static readonly Dictionary<ProjectType, List<string>> Guids = new Dictionary<ProjectType, List<string>>
        {
            {
                ProjectType.XamarinAndroid,
                new List<string>
                {
                    "EFBA0AD7-5A72-4C68-AF49-83D382785DCF"
                }
            },
            {
                ProjectType.XamarinIos,
                new List<string>
                {
                    "FEACFBD2-3405-455C-9665-78FE426C6842",
                    "6BC8ED88-2882-458C-8E55-DFD12B67127B"
                }
            },
            {
                ProjectType.Uwp,
                new List<string>
                {
                    "A5A43C5B-DE2A-4C0C-9213-0A381AF9435A"
                }
            }
        };
    }
}
