using System.Linq;
using System.Threading.Tasks;
using Dotnet.Deps.Core.NuGet;
using Dotnet.Deps.ProjectSystem;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.Commands
{
    public class ListPackagesCommand
    {
        private readonly AppConsole console;
        private readonly IProjectCollectionLoader projectCollectionLoader;
        private readonly ILatestVersionProvider latestVersionProvider;

        public ListPackagesCommand(AppConsole console, IProjectCollectionLoader projectCollectionLoader, ILatestVersionProvider latestVersionProvider)
        {
            this.console = console;
            this.projectCollectionLoader = projectCollectionLoader;
            this.latestVersionProvider = latestVersionProvider;
        }

        public async Task Execute(ListPackagesCommandOptions options)
        {
            var projectFiles = projectCollectionLoader.Load(options.RootFolder);

            var allPackageNames = projectFiles.SelectMany(pf => pf.NuGetPackageReferences.Select(npr => npr.Name)).Distinct().ToArray();
            console.WriteNormal($"Found {allPackageNames.Length} package references across {projectFiles.Length} project(s)");

            var latestVersions = await latestVersionProvider.GetLatestVersions(allPackageNames, options.RootFolder, options.PreRelease);

            foreach (var projectFile in projectFiles)
            {
                foreach (var packageReference in projectFile.NuGetPackageReferences)
                {
                    if (latestVersions.TryGetValue(packageReference.Name, out var latestVersion))
                    {
                        if (packageReference.NuGetVersion < latestVersion.NugetVersion)
                        {
                            if (options.Update)
                            {
                                packageReference.Update(new FloatRange(packageReference.FloatBehavior, latestVersion.NugetVersion).ToString());
                            }
                            console.WriteHighlighted($"{packageReference.Name} {packageReference.VersionString} => {latestVersion.NugetVersion} ({latestVersion.Feed}) 😢");
                        }
                        else
                        {
                            console.WriteSuccess($"{packageReference.Name} {packageReference.VersionString} (Feed: {latestVersion.Feed}) 🍺");
                        }
                    }
                    if (options.Update)
                    {
                        projectFile.Save();
                    }
                }

            }

        }
    }
}