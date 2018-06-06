using SemanticVersioning.Models;
using System.Linq;

namespace SemanticVersioning.Extensions
{
    public static class VersionExtensions
    {
        public static bool IsNullOrEmpty(this Version version)
        {
            if (version == null)
                return true;

            var properties = version.GetType().GetProperties();

            var targetProperties = properties.Where(x => x.GetValue(version) is int?);

            var values = targetProperties.Select(x => x.GetValue(version) as int?);

            var isEmpty = !values.Any(x => x != null);

            return isEmpty;
        }
    }
}
