using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dotnet.Deps.Core;
using McMaster.Extensions.CommandLineUtils;

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

            var cwd = app.Option("-cwd |--workingdirectory", "Working directory for analyzing dependencies. Defaults to current directory.", CommandOptionType.SingleValue);
            var filterOption = app.Option("-f | --filter", "Filter packages to be processed.", CommandOptionType.SingleValue);
            var versionOption = app.VersionOption("-v | --version", GetVersion());
            var preReleaseOption = app.Option("-p ||--pre", "Allow prerelease packages", CommandOptionType.NoValue);
            var updateOption = app.Option("-u ||--update", "Update packages to their latest versions", CommandOptionType.NoValue);
            var minAgeOption = app.Option("-ma | --min-age", "Minimum age of a package version before it is considered for an update. E.g. 2d (days) or 12h (hours). Defaults to days when no suffix is given.", CommandOptionType.SingleValue);
            var helpOption = app.HelpOption("-h | --help");

            app.OnExecuteAsync(async cancellationToken =>
            {
                TimeSpan? minimumAge = null;
                if (minAgeOption.HasValue())
                {
                    if (!MinimumAge.TryParse(minAgeOption.Value(), out var parsedMinimumAge))
                    {
                        console.WriteError($"Invalid value '{minAgeOption.Value()}' for the --min-age option. Expected for instance 2d (days), 12h (hours) or 2 (days).");
                        return 1;
                    }
                    minimumAge = parsedMinimumAge;
                }

                var results = await new DependencyAnalyzer()
                    .WithRootFolder(cwd.HasValue() ? cwd.Value() : Directory.GetCurrentDirectory())
                    .WithConsoleOutput(console)
                    .WithFilter(filterOption.Value())
                    .WithPreReleaseOption(preReleaseOption.HasValue())
                    .WithUpdateOption(updateOption.HasValue())
                    .WithMinimumAge(minimumAge)
                    .Execute();
                return results.Any(r => !r.IsLatestVersion) ? 0xbad : 0;
            });

            return app.Execute(args);
        }

        private string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
        }
    }
}