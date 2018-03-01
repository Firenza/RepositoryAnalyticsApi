using System.Text.RegularExpressions;

namespace RepositoryAnalyticsApi.Extensibility
{
    public static class StringExtensions
    {
        public static int? GetMajorVersion(this string version)
        {
            if (!string.IsNullOrWhiteSpace(version))
            {
                var match = Regex.Match(version, @"\d+");

                if (match.Success)
                {
                    return System.Int32.Parse(match.Value);
                }
            }

            return null;
        }
    }
}
