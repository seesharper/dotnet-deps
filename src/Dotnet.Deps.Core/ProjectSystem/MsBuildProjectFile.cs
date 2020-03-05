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

        public MsBuildProjectFile(XDocument msBuildProjectFile, string path, NuGetPackageReference[] nuGetPackageReferences)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            this.path = path;
            NuGetPackageReferences = nuGetPackageReferences;
        }

        public NuGetPackageReference[] NuGetPackageReferences { get; }

        public void Save()
        {
            foreach (var nuGetPackageReference in NuGetPackageReferences)
            {
                // Update the package versions.
            }
            msBuildProjectFile.Save(path);
        }
    }

}