using System.Collections.Generic;
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

            var properties = new Dictionary<string, MsBuildProperty>();
            var propertyGroups = projectFile.Descendants("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                foreach (var propertyElement in propertyGroup.Descendants())
                {
                    var value = propertyElement.Value;
                    var isVariable = propertyElement.Value.Trim().StartsWith("$");
                    var name = propertyElement.Name.LocalName;

                    if (!properties.ContainsKey(name))
                    {
                        properties.Add(name, new MsBuildProperty(name, value, isVariable));
                    }
                }
            }

            return new MsBuildProjectFile(projectFile, path, packageReferences.ToArray(), properties);
        }
    }

}