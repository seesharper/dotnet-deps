using System;
using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a MsBuild SDK-style project file.
    /// </summary>
    public class MsBuildProjectFile : IProjectFile<MsBuildPackageReference>
    {
        private readonly XDocument msBuildProjectFile;


        public MsBuildProjectFile(XDocument msBuildProjectFile, string path) : this(msBuildProjectFile, path, null)
        {
        }

        public MsBuildProjectFile(XDocument msBuildProjectFile, string path, TimeSpan? minimumPackageAge)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            Path = path;
            MinimumPackageAge = minimumPackageAge;
        }

        public MsBuildPackageReference[] PackageReferences { get; set; }

        public TimeSpan? MinimumPackageAge { get; }

        public string Path { get; }

        public void Save()
        {
            msBuildProjectFile.Save(Path);
        }
    }

}