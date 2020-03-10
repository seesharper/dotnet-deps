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




                //packageReferences.Add(new PackageReference(packageName, packageVersion));

                if (FloatRange.TryParse(packageVersion, out var floatRange))
                {
                    var nugetVersion = floatRange.MinVersion;
                    var nugetPackageReference = new MsBuildPackageReference(packageName, packageVersion, floatRange, packageReferenceElement);
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