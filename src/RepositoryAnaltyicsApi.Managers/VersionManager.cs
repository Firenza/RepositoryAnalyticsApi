using RepositoryAnaltyicsApi.Interfaces;
using System.Linq;
using System.Text;

namespace RepositoryAnaltyicsApi.Managers
{
    public class VersionManager : IVersionManager
    {
        public string GetMinorVersion(string version)
        {
            var versionChunks = version.Split('.');

            if (versionChunks != null && versionChunks.Length > 1)
            {
                return versionChunks.ElementAt(1);
            }
            else
            {
                return null;
            }
        }

        public string GetPaddedVersion(string version)
        {
            var versionStringBuilder = new StringBuilder();

            var padCharacter = ' ';
            // The total of actual version #'s plus padding for each section between the .'s
            var totalVersionChunkSize = 10;

            var versionChunks = version.Split('.');

            if (versionChunks != null && versionChunks.Any())
            {
                foreach (var versionChunk in versionChunks)
                {
                    if (versionStringBuilder.Length > 0)
                    {
                        versionStringBuilder.Append('.');
                    }

                    versionStringBuilder.Append(new string(padCharacter, totalVersionChunkSize - versionChunk.Length));
                    versionStringBuilder.Append(versionChunk);
                }

                return versionStringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
