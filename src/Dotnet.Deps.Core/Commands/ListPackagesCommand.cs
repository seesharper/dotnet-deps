using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Deps.Core.NuGet;
using Dotnet.Deps.Core.ProjectSystem;
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
            var projectFilesToSave = new List<IProjectFile>();

            var projectCollection = projectCollectionLoader.Load(options.RootFolder);
            var allPackages = projectCollection.ProjectFiles.SelectMany(pf => pf.PackageReferences).ToArray();
            var allPackageNames = allPackages.Select(pr => pr.Name).Distinct().ToArray();

            console.WriteNormal($"Found {allPackages.Length} package references across {projectCollection.ProjectFiles.Length} project(s)");

            var latestVersions = await latestVersionProvider.GetLatestVersions(allPackageNames, options.RootFolder, options.PreRelease);

            foreach (var projectFile in projectCollection.ProjectFiles)
            {
                console.WriteHeader(projectFile.Path);
                foreach (var packageReference in projectFile.PackageReferences)
                {
                    string packageVersion = null;

                    // if (packageReference.UsesVariable)
                    // {
                    //     var property = projectCollection.EvaluateVariable(packageReference.Version);
                    //     packageVersion = property.Value;
                    // }
                    // else
                    // {
                    packageVersion = packageReference.Version;
                    //}

                    if (FloatRange.TryParse(packageVersion, out var floatRange))
                    {
                        if (latestVersions.TryGetValue(packageReference.Name, out var latestVersion))
                        {
                            if (!latestVersion.IsValid)
                            {
                                continue;
                            }
                            if (!floatRange.Satisfies(latestVersion.NugetVersion))
                            {
                                console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) 😢");
                            }
                            else
                            {
                                console.WriteSuccess($"{packageReference.Name} {packageReference.Version} {latestVersion.NugetVersion} ({latestVersion.Feed}) 🍺");
                            }
                        }
                    }
                    else
                    {
                        console.WriteError($"Warning: The package '{packageReference.Name}' has an invalid version number '{packageVersion}'");
                    }
                }
            }



            // if (FloatRange.TryParse(packageVersion, out var floatRange))
            // {
            //     var nugetVersion = floatRange.MinVersion;
            //     var nugetPackageReference = new MsBuildPackageReference(packageName, packageVersion, floatRange.MinVersion, floatRange.FloatBehavior, packageReferenceElement);
            //     packageReferences.Add(nugetPackageReference);
            // }
            // else
            // {
            //     console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
            // }


            // foreach (var projectFile in projectFiles)
            // {
            //     foreach (var packageReference in projectFile.NuGetPackageReferences)
            //     {
            //         if (latestVersions.TryGetValue(packageReference.Name, out var latestVersion))
            //         {
            //             if (packageReference.NuGetVersion < latestVersion.NugetVersion)
            //             {
            //                 if (options.Update)
            //                 {
            //                     packageReference.Update(new FloatRange(packageReference.FloatBehavior, latestVersion.NugetVersion).ToString());
            //                 }
            //                 console.WriteHighlighted($"{packageReference.Name} {packageReference.VersionString} => {latestVersion.NugetVersion} ({latestVersion.Feed}) 😢");
            //             }
            //             else
            //             {
            //                 console.WriteSuccess($"{packageReference.Name} {packageReference.VersionString} (Feed: {latestVersion.Feed}) 🍺");
            //             }
            //         }
            //         if (options.Update)
            //         {
            //             projectFile.Save();
            //         }
            //     }

            // }

        }
    }
}