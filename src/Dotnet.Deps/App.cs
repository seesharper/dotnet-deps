using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dotnet.Deps.Core;
using Dotnet.Deps.Core.NuGet;
using Dotnet.Deps.Core.Commands;

using McMaster.Extensions.CommandLineUtils;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Dotnet.Deps.Core.ProjectSystem;

namespace Dotnet.Deps
{
    public class App
    {
        private readonly AppConsole console;

        public App(AppConsole console)
        {
            this.console = console;
        }

        public int Execute(params string[] args)
        {
            var app = new CommandLineApplication();
            var filterOption = app.Option("-f | --filter", "Filter packages to be processed.", CommandOptionType.SingleValue);

            var updateCommand = app.Command("update", c =>
            {
                c.Description = "Updates packages to their latest versions";
            });

            var versionOption = app.VersionOption("-v | --version", "1.0.1");
            var cwd = app.Option("-cwd |--workingdirectory <currentworkingdirectory>", "Working directory for analyzing dependencies. Defaults to current directory.", CommandOptionType.SingleValue);
            var preReleaseOption = app.Option("-p || --pre ", "Allow prerelease packages", CommandOptionType.NoValue);


            updateCommand.OnExecuteAsync(async cancellationToken =>
            {
                var workingDirectory = cwd.HasValue() ? cwd.Value() : Directory.GetCurrentDirectory();
                var listPackagesCommand = new ListPackagesCommand(console, new ProjectCollectionLoader(console, new IProjectLoader[] { new MsBuildProjectLoader(console), new ScriptProjectLoader(console) }), new LatestVersionProvider(console));
                var commandOptions = new ListPackagesCommandOptions(workingDirectory, "", "", false, true);
                await listPackagesCommand.Execute(commandOptions);
            });


            app.OnExecuteAsync(async cancellationToken =>
            {
                var workingDirectory = cwd.HasValue() ? cwd.Value() : Directory.GetCurrentDirectory();
                var listPackagesCommand = new ListPackagesCommand(console, new ProjectCollectionLoader(console, new IProjectLoader[] { new MsBuildProjectLoader(console), new ScriptProjectLoader(console) }), new LatestVersionProvider(console));
                var commandOptions = new ListPackagesCommandOptions(workingDirectory, "", "", false, false);
                await listPackagesCommand.Execute(commandOptions);
            });

            var helpOption = app.HelpOption("-h | --help");

            return app.Execute(args);
        }

        private async Task<int> ProcessPackages(string rootFolder, string filter, bool preRelease, bool update)
        {
            var packageReferences = GetPackageReferences(filter, rootFolder);
            var latestVersions = await GetLatestVersions(packageReferences.Select(r => r.Name).Distinct().ToArray(), rootFolder, preRelease);
            var packageReferencesGroupedByProject = packageReferences.Where(p => !latestVersions[p.Name].IsValid || latestVersions[p.Name].NugetVersion > p.Version).GroupBy(p => p.ProjectFile);

            foreach (var grouping in packageReferencesGroupedByProject)
            {
                console.WriteHeader(Path.GetRelativePath(rootFolder, grouping.Key));
                console.WriteEmptyLine();
                foreach (var packageReference in grouping)
                {
                    var latestVersion = latestVersions[packageReference.Name];
                    if (!latestVersion.IsValid)
                    {
                        console.WriteError($"Unable to find package {packageReference.Name} ({packageReference.Version})");
                        continue;
                    }
                    if (update)
                    {
                        SimpleExec.Command.Run("dotnet", $"add {packageReference.ProjectFile} package {packageReference.Name} -v {latestVersion.NugetVersion.ToString()}", noEcho: true);
                        console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed}) \u2705");
                    }
                    else
                    {
                        console.WriteHighlighted($"{packageReference.Name} {packageReference.Version} => {latestVersion.NugetVersion} ({latestVersion.Feed})");
                    }
                }

                console.WriteEmptyLine();
            }
            return 0;
        }

        private async Task<IDictionary<string, LatestVersion>> GetLatestVersions(string[] packageNames, string rootFolder, bool preRelease)
        {
            console.WriteHighlighted($"Getting the latest package versions. Hang on.....");

            var sourceRepositories = GetSourceRepositories(rootFolder);

            var result = new ConcurrentBag<LatestVersion>();

            await Task.WhenAll(packageNames.Select(name => GetLatestVersion(name, preRelease, sourceRepositories, result)));

            return result.ToDictionary(v => v.PackageName);
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

        private PackageReference_old[] GetPackageReferences(string filter, string rootFolder)
        {
            var projectFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);

            var packageReferences = new List<PackageReference_old>();
            foreach (var projectFile in projectFiles)
            {
                console.WriteNormal($"Analyzing {projectFile}");
                var content = File.ReadAllText(projectFile);
                var packagePattern = "PackageReference Include=\"([^\"]*)\" Version=\"([^\"]*)\"";
                var matcher = new Regex(packagePattern);
                var matches = matcher.Matches(content);
                foreach (var match in matches.Cast<Match>())
                {
                    var packageName = match.Groups[1].Value;
                    var version = match.Groups[2].Value;

                    if (!NuGetVersion.TryParse(version, out var nugetVersion))
                    {
                        var floatRange = FloatRange.Parse(version);
                        nugetVersion = floatRange.MinVersion;
                    }

                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        if (packageName.StartsWith(filter))
                        {
                            packageReferences.Add(new PackageReference_old(packageName.Trim(), projectFile, nugetVersion));
                        }
                    }
                    else
                    {
                        packageReferences.Add(new PackageReference_old(packageName.Trim(), projectFile, nugetVersion));
                    }
                }
            }
            return packageReferences.ToArray();
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

        private class PackageReference_old
        {
            public PackageReference_old(string name, string projectFile, NuGetVersion version)
            {
                Name = name;
                ProjectFile = projectFile;
                Version = version;
            }

            public string Name { get; }
            public string ProjectFile { get; }
            public NuGetVersion Version { get; }
        }


    }
}