using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ShellProgressBar;

namespace Dotnet.Deps.Core.NuGet
{
    public interface ILatestVersionProvider
    {
        Task<IDictionary<string, LatestVersion>> GetLatestVersions(string[] packageNames, string rootFolder, bool preRelease);

        /// <summary>
        /// Gets the candidate versions for the given packages.
        /// </summary>
        /// <param name="includePublishedDates">
        /// When <c>true</c>, all versions are returned along with their publish date so that they can be filtered by age.
        /// When <c>false</c>, only the latest version per feed is returned, which is considerably cheaper.
        /// </param>
        Task<IDictionary<string, PackageVersions>> GetPackageVersions(string[] packageNames, string rootFolder, bool preRelease, bool includePublishedDates);
    }

    public class LatestVersionProvider : ILatestVersionProvider
    {
        private readonly AppConsole console;

        public LatestVersionProvider(AppConsole console)
        {
            this.console = console;
        }

        public async Task<IDictionary<string, LatestVersion>> GetLatestVersions(string[] packageNames, string rootFolder, bool preRelease)
        {
            var packageVersions = await GetPackageVersions(packageNames, rootFolder, preRelease, false).ConfigureAwait(false);
            var utcNow = DateTimeOffset.UtcNow;
            return packageVersions.ToDictionary(pv => pv.Key, pv => pv.Value.GetLatestVersion(null, utcNow));
        }

        public async Task<IDictionary<string, PackageVersions>> GetPackageVersions(string[] packageNames, string rootFolder, bool preRelease, bool includePublishedDates)
        {
            console.WriteHighlighted($"Getting the latest package versions. Hang on.....");

            var sourceRepositories = GetSourceRepositories(rootFolder);

            var result = new ConcurrentBag<PackageVersions>();

            int totalTicks = packageNames.Length;
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var progressBar = new ProgressBar(totalTicks, "Getting latest package versions", options))
            {
                await Task.WhenAll(packageNames.Select(name => GetPackageVersions(name, preRelease, includePublishedDates, sourceRepositories, result, progressBar))).ConfigureAwait(false);
            }

            return result.ToDictionary(v => v.PackageName);
        }


        private SourceRepository[] GetSourceRepositories(string rootFolder)
        {
            var provider = GetSourceRepositoryProvider(rootFolder);
            var repositories = provider.GetRepositories().ToArray();
            console.WriteNormal("Feeds");
            console.WriteEmptyLine();
            foreach (var repository in repositories)
            {
                console.WriteNormal($" * {repository.PackageSource.ToString()}");
            }

            console.WriteEmptyLine();

            return repositories.ToArray();
        }

        private static ISourceRepositoryProvider GetSourceRepositoryProvider(string rootFolder)
        {
            var settings = global::NuGet.Configuration.Settings.LoadDefaultSettings(rootFolder);
            var packageSourceProvider = new PackageSourceProvider(settings);
            return new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
        }

        private async Task GetPackageVersions(string packageName, bool preRelease, bool includePublishedDates, SourceRepository[] repositories, ConcurrentBag<PackageVersions> result, ProgressBar progressBar)
        {
            List<PackageVersion> allVersions = new List<PackageVersion>();
            foreach (var repository in repositories)
            {
                if (includePublishedDates)
                {
                    allVersions.AddRange(await GetVersionsWithPublishedDate(packageName, preRelease, repository).ConfigureAwait(false));
                }
                else
                {
                    var latestVersionInRepository = await GetLatestVersionInRepository(packageName, preRelease, repository).ConfigureAwait(false);
                    if (latestVersionInRepository != null)
                    {
                        allVersions.Add(new PackageVersion(latestVersionInRepository, null, repository.ToString()));
                    }
                }
            }

            result.Add(new PackageVersions(packageName, allVersions));
            progressBar.Tick(packageName);
        }

        private static async Task<NuGetVersion> GetLatestVersionInRepository(string packageName, bool preRelease, SourceRepository repository)
        {
            var findResource = repository.GetResource<FindPackageByIdResource>();
            var allVersions = await findResource.GetAllVersionsAsync(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None).ConfigureAwait(false);

            if (preRelease)
            {
                return allVersions.OrderBy(nv => nv).LastOrDefault();
            }

            return allVersions.Where(v => !v.IsPrerelease).OrderBy(nv => nv).LastOrDefault();
        }

        private static async Task<IEnumerable<PackageVersion>> GetVersionsWithPublishedDate(string packageName, bool preRelease, SourceRepository repository)
        {
            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>().ConfigureAwait(false);
            var metadata = await metadataResource.GetMetadataAsync(packageName, preRelease, false, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None).ConfigureAwait(false);

            return metadata
                .Where(m => preRelease || !m.Identity.Version.IsPrerelease)
                .Select(m => new PackageVersion(m.Identity.Version, m.Published, repository.ToString()))
                .ToArray();
        }
    }
}
