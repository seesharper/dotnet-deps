using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.NuGet
{
    public interface ILatestVersionProvider
    {
        Task<IDictionary<string, LatestVersion>> GetLatestVersions(string[] packageNames, string rootFolder, bool preRelease);
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
            console.WriteHighlighted($"Getting the latest package versions. Hang on.....");

            var sourceRepositories = GetSourceRepositories(rootFolder);

            var result = new ConcurrentBag<LatestVersion>();

            await Task.WhenAll(packageNames.Select(name => GetLatestVersion(name, preRelease, sourceRepositories, result)));

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
            return new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());
        }

        private static async Task GetLatestVersion(string packageName, bool preRelease, SourceRepository[] repositories, ConcurrentBag<LatestVersion> result)
        {
            List<LatestVersion> allLatestVersions = new List<LatestVersion>();
            foreach (var repository in repositories)
            {
                var findResource = repository.GetResource<FindPackageByIdResource>();
                var allVersions = await findResource.GetAllVersionsAsync(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);
                NuGetVersion latestVersionInRepository;

                if (preRelease)
                {
                    latestVersionInRepository = allVersions.LastOrDefault();
                }
                else
                {
                    latestVersionInRepository = allVersions.Where(v => !v.IsPrerelease).LastOrDefault();
                }

                if (latestVersionInRepository != null)
                {
                    allLatestVersions.Add(new LatestVersion(packageName, latestVersionInRepository, repository.ToString()));
                }
            }
            if (!allLatestVersions.Any())
            {
                result.Add(new LatestVersion(packageName));
            }
            else
            {
                result.Add(allLatestVersions.OrderBy(lv => lv.NugetVersion).Last());
            }
        }
    }
}