using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.NuGet
{
    /// <summary>
    /// Represents a single version of a NuGet package found in a given feed.
    /// </summary>
    public class PackageVersion
    {
        public PackageVersion(NuGetVersion version, DateTimeOffset? published, string feed)
        {
            Version = version;
            Published = published;
            Feed = feed;
        }

        public NuGetVersion Version { get; }

        /// <summary>
        /// Gets the point in time when this version was published or <c>null</c> if the feed does not report it.
        /// </summary>
        public DateTimeOffset? Published { get; }

        public string Feed { get; }
    }

    /// <summary>
    /// Represents the candidate versions of a NuGet package across all configured feeds.
    /// </summary>
    public class PackageVersions
    {
        private readonly PackageVersion[] versions;

        public PackageVersions(string packageName, IEnumerable<PackageVersion> versions)
        {
            PackageName = packageName;
            this.versions = versions.OrderBy(v => v.Version).ToArray();
        }

        public string PackageName { get; }

        public IReadOnlyList<PackageVersion> Versions { get => versions; }

        public bool HasVersions { get => versions.Length > 0; }

        /// <summary>
        /// Gets the latest version that is at least <paramref name="minimumAge"/> old.
        /// Versions for which the feed does not report a publish date are never held back.
        /// </summary>
        public LatestVersion GetLatestVersion(TimeSpan? minimumAge, DateTimeOffset utcNow)
        {
            IEnumerable<PackageVersion> candidates = versions;
            if (minimumAge.HasValue && minimumAge.Value > TimeSpan.Zero)
            {
                var cutOff = utcNow - minimumAge.Value;
                candidates = candidates.Where(v => !v.Published.HasValue || v.Published.Value <= cutOff);
            }

            var latestVersion = candidates.LastOrDefault();
            if (latestVersion == null)
            {
                return new LatestVersion(PackageName);
            }

            return new LatestVersion(PackageName, latestVersion.Version, latestVersion.Feed);
        }
    }
}
