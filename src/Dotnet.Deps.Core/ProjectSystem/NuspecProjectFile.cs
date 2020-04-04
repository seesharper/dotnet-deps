using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a MsBuild SDK-style project file.
    /// </summary>
    public class NuspecProjectFile : IProjectFile<NuspecPackageReference>
    {
        private readonly XDocument msBuildProjectFile;


        public NuspecProjectFile(XDocument msBuildProjectFile, string path)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            Path = path;
        }

        public NuspecPackageReference[] PackageReferences { get; set; }

        public string Path { get; }

        public void Save()
        {
            msBuildProjectFile.Save(Path);
        }
    }

}