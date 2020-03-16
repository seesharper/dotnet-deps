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

            int totalTicks = packageNames.Length;
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var progressBar = new ProgressBar(totalTicks, "Getting latest package versions", options))
            {
                await Task.WhenAll(packageNames.Select(name => GetLatestVersion(name, preRelease, sourceRepositories, result, progressBar))).ConfigureAwait(false);
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

        private async Task GetLatestVersion(string packageName, bool preRelease, SourceRepository[] repositories, ConcurrentBag<LatestVersion> result, ProgressBar progressBar)
        {
            List<LatestVersion> allLatestVersions = new List<LatestVersion>();
            foreach (var repository in repositories)
            {
                var findResource = repository.GetResource<FindPackageByIdResource>();
                var allVersions = await findResource.GetAllVersionsAsync(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);
                NuGetVersion latestVersionInRepository;

                if (preRelease)
                {
                    latestVersionInRepository = allVersions.OrderBy(nv => nv).LastOrDefault();
                }
                else
                {
                    latestVersionInRepository = allVersions.Where(v => !v.IsPrerelease).OrderBy(nv => nv).LastOrDefault();
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
            progressBar.Tick(packageName);
        }
    }
}