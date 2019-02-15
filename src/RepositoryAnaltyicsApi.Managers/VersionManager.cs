using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Linq;
using System.Text;

namespace RepositoryAnaltyicsApi.Managers
{
    public class VersionManager : IVersionManager
    {
        public string GetMinorVersion(string version)
        {
            string minorVersion = null;

            if (version != null)
            {
                var versionChunks = version.Split('.');

                if (versionChunks != null && versionChunks.Length > 1)
                {
                    minorVersion =  versionChunks.ElementAt(1);
                }
            }

            return minorVersion;
        }

        public string GetPaddedVersion(string version)
        {
            string paddedVersion = null;

            var versionStringBuilder = new StringBuilder();

            var padCharacter = ' ';
            // The total of actual version #'s plus padding for each section between the .'s
            var totalVersionChunkSize = 10;

            if (version != null)
            {
                var versionChunks = version.Split('.');

                if (versionChunks != null && versionChunks.Any())
                {
                    foreach (var versionChunk in versionChunks)
                    {
                        if (versionStringBuilder.Length > 0)
                        {
                            versionStringBuilder.Append('.');
                        }

                        if (versionChunk.Length <= totalVersionChunkSize)
                        {
                            versionStringBuilder.Append(new string(padCharacter, totalVersionChunkSize - versionChunk.Length));
                            versionStringBuilder.Append(versionChunk);
                        }
                        else
                        {
                            throw new ArgumentException($"Version chunk of {versionChunk} is longer than the max length of {totalVersionChunkSize}");
                        }
                    }

                    paddedVersion = versionStringBuilder.ToString();
                }
            }

            return paddedVersion;
        }
    }
}
