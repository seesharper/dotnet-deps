using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a MsBuild SDK-style project file.
    /// </summary>
    public class MsBuildProjectFile : IProjectFile<MsBuildPackageReference>
    {
        private readonly XDocument msBuildProjectFile;


        public MsBuildProjectFile(XDocument msBuildProjectFile, string path)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            Path = path;
        }

        public MsBuildPackageReference[] PackageReferences { get; set; }

        public string Path { get; }

        public void Save()
        {
            msBuildProjectFile.Save(Path);
        }
    }

}