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

        private TimeSpan? minimumAge;

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

        /// <summary>
        /// Specifies the minimum age a package version must have before it is considered for an update.
        /// Takes precedence over the <c>PackagesMinimumAge</c> property in the project file.
        /// </summary>
        public DependencyAnalyzer WithMinimumAge(TimeSpan? minimumAge)
        {
            this.minimumAge = minimumAge;
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

            var minimumAgeIsInEffect = minimumAge.HasValue || projectCollection.ProjectFiles.Any(pf => pf.MinimumPackageAge.HasValue);
            var packageVersions = await latestVersionProvider.GetPackageVersions(allPackageNames, rootFolder, allowPreReleasePackages, minimumAgeIsInEffect);
            var utcNow = DateTimeOffset.UtcNow;

            foreach (var projectFile in projectCollection.ProjectFiles)
            {
                console.WriteHeader(projectFile.Path);

                var effectiveMinimumAge = minimumAge ?? projectFile.MinimumPackageAge;
                if (effectiveMinimumAge.HasValue && effectiveMinimumAge.Value > TimeSpan.Zero)
                {
                    console.WriteNormal($"Ignoring package versions published less than {MinimumAge.Format(effectiveMinimumAge.Value)} ago ⏳");
                }

                foreach (var packageReference in projectFile.PackageReferences)
                {
                    if (!Regex.IsMatch(packageReference.Name, filter))
                    {
                        continue;
                    }


                    string packageVersion = null;
                    packageVersion = packageReference.Version;

                    if (packageReference.Locked)
                    {
                        console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} LOCKED 🔒");
                        continue;
                    }


                    if (FloatRange.TryParse(packageVersion, out var floatRange))
                    {
                        if (packageVersions.TryGetValue(packageReference.Name, out var availableVersions))
                        {
                            var latestVersion = availableVersions.GetLatestVersion(effectiveMinimumAge, utcNow);

                            if (!latestVersion.IsValid)
                            {
                                if (availableVersions.HasVersions)
                                {
                                    console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} - no version is older than {MinimumAge.Format(effectiveMinimumAge.Value)} ⏳");
                                }
                                else
                                {
                                    console.WriteError($"Unable to find package {packageReference.Name} ({packageReference.Version})");
                                }
                                continue;
                            }

                            var heldBack = GetHeldBackSuffix(availableVersions, latestVersion, effectiveMinimumAge, utcNow);

                            if (!IsLatestVersion(floatRange, latestVersion.NugetVersion))
                            {
                                if (updateDependencies)
                                {
                                    console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) UPDATED 🍺{heldBack}");
                                    packageReference.Update(latestVersion.NugetVersion.ToString());
                                    results.Add(new Result(floatRange.MinVersion.ToString(), latestVersion.NugetVersion.ToString(), true, latestVersion.Feed, projectFile.Path));
                                }
                                else
                                {
                                    results.Add(new Result(floatRange.MinVersion.ToString(), latestVersion.NugetVersion.ToString(), false, latestVersion.Feed, projectFile.Path));
                                    console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) 😢{heldBack}");
                                }
                            }
                            else
                            {
                                results.Add(new Result(floatRange.MinVersion.ToString(), latestVersion.NugetVersion.ToString(), true, latestVersion.Feed, projectFile.Path));
                                console.WriteSuccess($"{packageReference.Name} {packageReference.Version} {latestVersion.NugetVersion} ({latestVersion.Feed}) 🍺{heldBack}");
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

        private static string GetHeldBackSuffix(PackageVersions availableVersions, LatestVersion latestVersion, TimeSpan? effectiveMinimumAge, DateTimeOffset utcNow)
        {
            if (!effectiveMinimumAge.HasValue || effectiveMinimumAge.Value <= TimeSpan.Zero)
            {
                return string.Empty;
            }

            var newestVersion = availableVersions.GetLatestVersion(null, utcNow);
            if (newestVersion.IsValid && newestVersion.NugetVersion > latestVersion.NugetVersion)
            {
                return $" (holding back {newestVersion.NugetVersion} ⏳)";
            }

            return string.Empty;
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