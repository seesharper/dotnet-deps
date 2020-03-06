using System.Collections.Generic;
using System.Xml.Linq;

namespace Dotnet.Deps.ProjectSystem
{
    /// <summary>
    /// Represents a MsBuild SDK-style project file.
    /// </summary>
    public class MsBuildProjectFile : IProjectFile
    {
        private readonly XDocument msBuildProjectFile;
        private readonly string path;

        public MsBuildProjectFile(XDocument msBuildProjectFile, string path, NuGetPackageReference[] nuGetPackageReferences, IDictionary<string, MsBuildProperty> properties)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            this.path = path;
            NuGetPackageReferences = nuGetPackageReferences;
            Properties = properties;
        }

        public NuGetPackageReference[] NuGetPackageReferences { get; }

        public IDictionary<string, MsBuildProperty> Properties { get; }

        public void Save()
        {
            foreach (var nuGetPackageReference in NuGetPackageReferences)
            {
                // Update the package versions.
            }
            msBuildProjectFile.Save(path);
        }
    }


    public class MsBuildProperty
    {
        public MsBuildProperty(string name, string value, bool isVariable)
        {
            Name = name;
            Value = value;
            IsVariable = isVariable;
        }

        public string Name { get; }

        public string Value { get; }

        public bool IsVariable { get; }
    }
}