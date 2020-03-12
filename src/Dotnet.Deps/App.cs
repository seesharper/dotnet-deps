using System;
using System.IO;
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
            var versionOption = app.VersionOption("-v | --version", "1.0.1");
            var preReleaseOption = app.Option("-p ||--pre", "Allow prerelease packages", CommandOptionType.NoValue);
            var updateOption = app.Option("-u ||--update", "Update packages to their latest versions", CommandOptionType.NoValue);
            var helpOption = app.HelpOption("-h | --help");

            app.OnExecuteAsync(async cancellationToken =>
            {
                await new DependencyAnalyzer()
                    .WithRootFolder(cwd.HasValue() ? cwd.Value() : Directory.GetCurrentDirectory())
                    .WithConsoleOutput(console)
                    .WithFilter(filterOption.Value())
                    .WithPreReleaseOption(preReleaseOption.HasValue())
                    .WithUpdateOption(updateOption.HasValue())
                    .Execute();
            });

            return app.Execute(args);
        }
    }
}