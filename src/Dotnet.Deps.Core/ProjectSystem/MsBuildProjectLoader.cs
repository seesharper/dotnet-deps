using System.Collections.Generic;
using System.Xml.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class MsBuildProjectLoader : IProjectLoader
    {
        private readonly AppConsole console;

        public MsBuildProjectLoader(AppConsole console)
        {
            this.console = console;
        }

        public string FileExtensions { get => "csproj;props;target"; }

        public IProjectFile<NuGetPackageReference> Load(string path)
        {
            var projectFile = XDocument.Load(path);
            var nameSpace = projectFile.Root.Name.Namespace;
            var msBuildProjectFile = new MsBuildProjectFile(projectFile, path);
            var packageReferenceElements = projectFile.Descendants(nameSpace + "PackageReference");
            var packageReferences = new List<MsBuildPackageReference>();
            foreach (var packageReferenceElement in packageReferenceElements)
            {
                var packageName = packageReferenceElement.Attribute("Include")?.Value;
                var lockedValue = packageReferenceElement.Attribute("Locked")?.Value;
                var lockedVersion = false;
                if (lockedValue != null)
                {
                    lockedVersion = bool.TryParse(lockedValue, out var locked) && locked;
                }

                if (packageName == null)
                {
                    packageName = packageReferenceElement.Attribute("Update")?.Value;
                }
                if (string.IsNullOrWhiteSpace(packageName))
                {
                    continue;
                }

                var packageVersion = packageReferenceElement.Attribute("Version")?.Value;
                if (packageVersion == null || packageVersion.StartsWith("$"))
                {
                    continue;
                }

                if (FloatRange.TryParse(packageVersion, out var floatRange))
                {
                    var nugetPackageReference = new MsBuildPackageReference(packageName, packageVersion, lockedVersion, floatRange, packageReferenceElement);
                    packageReferences.Add(nugetPackageReference);
                }
                else
                {
                    console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
                }
            }

            msBuildProjectFile.PackageReferences = packageReferences.ToArray();


            return msBuildProjectFile;
        }
    }

}