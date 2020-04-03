using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dotnet.Deps.Core.NuGet;
using Dotnet.Deps.Core.ProjectSystem;
using NuGet.Versioning;

namespace Dotnet.Deps.Core
{
    public class DependencyAnalyzer
    {
        private string rootFolder;

        private string filter = ".*";

        private bool updateDependencies;

        private bool allowPreReleasePackages;

        private AppConsole console = new AppConsole(TextWriter.Null, TextWriter.Null);

        public DependencyAnalyzer WithRootFolder(string rootFolder)
        {
            this.rootFolder = rootFolder;
            return this;
        }

        public DependencyAnalyzer WithFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                filter = ".*";
            }
            this.filter = filter;
            return this;
        }

        public DependencyAnalyzer WithConsoleOutput(AppConsole console)
        {
            this.console = console;
            return this;
        }

        public DependencyAnalyzer WithUpdateOption(bool updateDependencies)
        {
            this.updateDependencies = updateDependencies;
            return this;
        }

        public DependencyAnalyzer WithPreReleaseOption(bool allowPreReleasePackages)
        {
            this.allowPreReleasePackages = allowPreReleasePackages;
            return this;
        }

        public async Task<Result[]> Execute()
        {
            var projectCollectionLoader = new ProjectCollectionLoader(console);
            var latestVersionProvider = new LatestVersionProvider(console);
            List<Result> results = new List<Result>();

            var projectFilesToSave = new HashSet<IProjectFile<NuGetPackageReference>>();

            var projectCollection = projectCollectionLoader.Load(rootFolder);
            var allPackages = projectCollection.ProjectFiles.SelectMany(pf => pf.PackageReferences).ToArray();
            var allPackageNames = allPackages.Select(pr => pr.Name).Distinct().ToArray();

            console.WriteNormal($"Found {allPackages.Length} package references across {projectCollection.ProjectFiles.Length} project(s)");

            var latestVersions = await latestVersionProvider.GetLatestVersions(allPackageNames, rootFolder, allowPreReleasePackages);

            foreach (var projectFile in projectCollection.ProjectFiles)
            {
                console.WriteHeader(projectFile.Path);
                foreach (var packageReference in projectFile.PackageReferences)
                {
                    if (!Regex.IsMatch(packageReference.Name, filter))
                    {
                        continue;
                    }


                    string packageVersion = null;
                    packageVersion = packageReference.Version;


                    if (FloatRange.TryParse(packageVersion, out var floatRange))
                    {
                        if (latestVersions.TryGetValue(packageReference.Name, out var latestVersion))
                        {
                            if (!latestVersion.IsValid)
                            {
                                console.WriteError($"Unable to find package {packageReference.Name} ({packageReference.Version})");
                                continue;
                            }

                            if (!IsLatestVersion(floatRange, latestVersion.NugetVersion))
                            {
                                results.Add(new Result(floatRange.MinVersion.ToString(), latestVersion.NugetVersion.ToString(), false, latestVersion.Feed, projectFile.Path));

                                if (updateDependencies)
                                {
                                    console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) UPDATED 🍺");
                                    packageReference.Update(latestVersion.NugetVersion.ToString());
                                }
                                else
                                {
                                    console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) 😢");
                                }
                            }
                            else
                            {
                                results.Add(new Result(floatRange.MinVersion.ToString(), latestVersion.NugetVersion.ToString(), true, latestVersion.Feed, projectFile.Path));
                                console.WriteSuccess($"{packageReference.Name} {packageReference.Version} {latestVersion.NugetVersion} ({latestVersion.Feed}) 🍺");
                            }
                        }
                    }
                    else
                    {
                        console.WriteError($"Warning: The package '{packageReference.Name}' has an invalid version number '{packageVersion}'");
                    }
                }
                if (updateDependencies)
                {
                    projectFile.Save();
                }
                else
                {
                    var numberOfOutDatedDependencies = results.Count(r => !r.IsLatestVersion);
                    if (numberOfOutDatedDependencies > 0)
                    {
                        console.WriteEmptyLine();
                        console.WriteNormal($"We found {numberOfOutDatedDependencies} 😢 dependencies. For 🍺, type 'deps --update'");
                    }
                }
            }

            return results.ToArray();
        }

        private bool IsLatestVersion(FloatRange currentVersion, NuGetVersion latestVersion)
        {
            if (currentVersion.FloatBehavior == NuGetVersionFloatBehavior.None)
            {
                return currentVersion.MinVersion >= latestVersion;
            }
            else
            {
                return currentVersion.Satisfies(latestVersion);
            }
        }
    }

    public class Result
    {
        public Result(string currentVersion, string latestVersion, bool isLatestVersion, string feed, string project)
        {
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
            IsLatestVersion = isLatestVersion;
            Feed = feed;
            Project = project;
        }

        public string CurrentVersion { get; }

        public string LatestVersion { get; }

        public bool IsLatestVersion { get; }

        public string Feed { get; }

        public string Project { get; }
    }
}