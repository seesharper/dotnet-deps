using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dotnet.Deps.Core;
using NuGet.Versioning;

namespace Dotnet.Deps.ProjectSystem
{

    public class MsBuildProjectLoader : IProjectLoader
    {
        private readonly AppConsole console;

        public MsBuildProjectLoader(AppConsole console)
        {
            this.console = console;
        }

        public string FileExtension { get => "csproj"; }

        public IProjectFile Load(string path)
        {
            var projectFile = XDocument.Load(path);
            var packageReferenceElements = projectFile.Descendants("PackageReference");
            var packageReferences = new List<NuGetPackageReference>();
            foreach (var packageReferenceElement in packageReferenceElements)
            {
                var packageName = packageReferenceElement.Attribute("Include").Value;
                var packageVersion = packageReferenceElement.Attribute("Version")?.Value;
                if (packageVersion == null)
                {
                    continue;
                }
                if (FloatRange.TryParse(packageVersion, out var floatRange))
                {
                    var nugetVersion = floatRange.MinVersion;
                    var nugetPackageReference = new MsBuildPackageReference(packageName, packageVersion, floatRange.MinVersion, floatRange.FloatBehavior, packageReferenceElement);
                    packageReferences.Add(nugetPackageReference);
                }
                else
                {
                    console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
                }
            }

            return new MsBuildProjectFile(projectFile, path, packageReferences.ToArray());
        }
    }

    public interface IProjectCollectionLoader
    {
        IProjectFile[] Load(string path);
    }

    public class ProjectCollectionLoader : IProjectCollectionLoader
    {
        private readonly AppConsole console;
        private readonly IProjectLoader projectLoader;

        public ProjectCollectionLoader(AppConsole console, IProjectLoader projectLoader)
        {
            this.console = console;
            this.projectLoader = projectLoader;
        }

        public IProjectFile[] Load(string workingDirectory)
        {
            var projectFiles = Directory.GetFiles(workingDirectory, $"*.{projectLoader.FileExtension}", SearchOption.AllDirectories);
            var result = new List<IProjectFile>();
            foreach (var projectFile in projectFiles)
            {
                console.WriteNormal($"Analyzing {projectFile}");
                result.Add(projectLoader.Load(projectFile));
            }

            return result.ToArray(); ;
        }
    }

}