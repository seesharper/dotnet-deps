namespace Dotnet.Deps.Core.Commands
{
    public class ListPackagesCommandOptions
    {
        public ListPackagesCommandOptions(string rootFolder, string packageFilter, string projectFilter, bool preRelease, bool update)
        {
            RootFolder = rootFolder;
            PackageFilter = packageFilter;
            ProjectFilter = projectFilter;
            PreRelease = preRelease;
            Update = update;
        }

        public string RootFolder { get; }

        public string PackageFilter { get; }

        public string ProjectFilter { get; }

        public bool PreRelease { get; }
        public bool Update { get; }
    }
}