using System.Collections.Generic;
using System.Xml.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class NuspecProjectLoader : IProjectLoader
    {
        private readonly AppConsole console;

        public NuspecProjectLoader(AppConsole console)
        {
            this.console = console;
        }

        public string FileExtensions { get => "nuspec"; }

        public IProjectFile<NuGetPackageReference> Load(string path)
        {
            var projectFile = XDocument.Load(path);
            var nameSpace = projectFile.Root.Name.Namespace;
            var nugetSpecProjectFile = new NuspecProjectFile(projectFile, path);
            var packageReferenceElements = projectFile.Descendants(nameSpace + "dependency");
            var packageReferences = new List<NuspecPackageReference>();
            foreach (var packageReferenceElement in packageReferenceElements)
            {
                var packageName = packageReferenceElement.Attribute("id")?.Value;
                if (string.IsNullOrWhiteSpace(packageName))
                {
                    continue;
                }

                var packageVersion = packageReferenceElement.Attribute("version")?.Value;
                if (packageVersion == null)
                {
                    continue;
                }

                if (FloatRange.TryParse(packageVersion, out var floatRange))
                {
                    var nugetVersion = floatRange.MinVersion;
                    var nugetPackageReference = new NuspecPackageReference(packageName, packageVersion, floatRange, packageReferenceElement);
                    packageReferences.Add(nugetPackageReference);
                }
                else
                {
                    console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
                }
            }

            nugetSpecProjectFile.PackageReferences = packageReferences.ToArray();


            return nugetSpecProjectFile;
        }
    }

}