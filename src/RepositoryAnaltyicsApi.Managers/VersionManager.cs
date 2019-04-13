using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            string preReleaseVersion = GetPreReleaseVersion(version);

            var versionStringBuilder = new StringBuilder();

            var padCharacter = '0';
            // The total of actual version #'s plus padding for each section between the .'s
            var totalVersionChunkSize = 10;
            var totalPreReleaseVersionChunkSize = 50;

            if (version != null)
            {
                var versionChunks = version.Split('.');

                if (versionChunks != null && versionChunks.Any())
                {
                    foreach (var versionChunk in versionChunks)
                    {
                        var versionChunkToProcess = versionChunk;

                        if (preReleaseVersion != null && versionChunk.EndsWith(preReleaseVersion))
                        {
                            versionChunkToProcess = versionChunk.Replace($"-{preReleaseVersion}", string.Empty);
                        }

                        if (versionStringBuilder.Length > 0)
                        {
                            versionStringBuilder.Append('.');
                        }

                        if (versionChunkToProcess.Length <= totalVersionChunkSize)
                        {
                            versionStringBuilder.Append(new string(padCharacter, totalVersionChunkSize - versionChunkToProcess.Length));
                            versionStringBuilder.Append(versionChunkToProcess);
                        }
                        else
                        {
                            throw new ArgumentException($"Version chunk of {versionChunkToProcess} is longer than the max length of {totalVersionChunkSize}");
                        }
                    }

                    if (preReleaseVersion != null)
                    {
                        versionStringBuilder.Append('-');

                        if (preReleaseVersion.Length <= totalPreReleaseVersionChunkSize)
                        {
                            versionStringBuilder.Append(new string(padCharacter, totalPreReleaseVersionChunkSize - preReleaseVersion.Length));
                            versionStringBuilder.Append(preReleaseVersion);
                        }
                        else
                        {
                            throw new ArgumentException($"Pre-release version chunk of {preReleaseVersion} is longer than the max length of {totalPreReleaseVersionChunkSize}");
                        }

                    }

                    paddedVersion = versionStringBuilder.ToString();
                }
            }

            return paddedVersion;
        }

        public string GetPreReleaseVersion(string version)
        {
            string preReleaseVersion = null;

            var match = Regex.Match(version, @"\.\d+-(.*\z)");

            if (match.Success)
            {
                preReleaseVersion = match.Groups[1].Value;
            };

            return preReleaseVersion;
        }
    }
}
