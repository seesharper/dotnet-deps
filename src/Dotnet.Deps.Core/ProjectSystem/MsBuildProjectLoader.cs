using System.Collections.Generic;
using System.Xml.Linq;

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

        public IProjectFile Load(string path)
        {
            var projectFile = XDocument.Load(path);
            var msBuildProjectFile = new MsBuildProjectFile(projectFile, path);
            var packageReferenceElements = projectFile.Descendants("PackageReference");
            var packageReferences = new List<PackageReference>();
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
                if (packageVersion == null)
                {
                    continue;
                }

                var usesVariable = packageVersion.StartsWith("$");

                packageReferences.Add(new PackageReference(packageName, packageVersion, usesVariable));

                // if (FloatRange.TryParse(packageVersion, out var floatRange))
                // {
                //     var nugetVersion = floatRange.MinVersion;
                //     var nugetPackageReference = new MsBuildPackageReference(packageName, packageVersion, floatRange.MinVersion, floatRange.FloatBehavior, packageReferenceElement);
                //     packageReferences.Add(nugetPackageReference);
                // }
                // else
                // {
                //     console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
                // }
            }

            var properties = new List<Property>();
            var propertyGroups = projectFile.Descendants("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                foreach (var propertyElement in propertyGroup.Descendants())
                {
                    var value = propertyElement.Value;
                    var isVariable = propertyElement.Value.Trim().StartsWith("$");
                    var name = propertyElement.Name.LocalName;
                    properties.Add(new Property(name, value, isVariable, msBuildProjectFile));
                }
            }

            msBuildProjectFile.PackageReferences = packageReferences.ToArray();
            msBuildProjectFile.Properties = properties.ToArray();

            return msBuildProjectFile;
        }
    }

}