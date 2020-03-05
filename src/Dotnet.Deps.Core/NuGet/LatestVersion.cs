using NuGet.Versioning;

namespace Dotnet.Deps.Core.NuGet
{
    /// <summary>
    /// Represents the latest version of a NuGet package
    /// </summary>
    public class LatestVersion
    {
        public LatestVersion(string packageName, NuGetVersion nugetVersion, string feed)
        {
            PackageName = packageName;
            NugetVersion = nugetVersion;
            Feed = feed;
        }

        public LatestVersion(string packageName) => PackageName = packageName;

        public string PackageName { get; }
        public NuGetVersion NugetVersion { get; }
        public string Feed { get; }

        public bool IsValid { get => !string.IsNullOrWhiteSpace(Feed); }
    }
}