using System.Collections.Generic;
using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a MsBuild SDK-style project file.
    /// </summary>
    public class MsBuildProjectFile : IProjectFile
    {
        private readonly XDocument msBuildProjectFile;
        private readonly string path;

        public MsBuildProjectFile(XDocument msBuildProjectFile, string path)
        {
            this.msBuildProjectFile = msBuildProjectFile;
            this.path = path;
        }

        public PackageReference[] PackageReferences { get; set; }

        public Property[] Properties { get; set; }

        public void Save()
        {
            msBuildProjectFile.Save(path);
        }
    }


    public class Property
    {
        public Property(string name, string value, bool isVariable, IProjectFile projectFile)
        {
            Name = name;
            Value = value;
            IsVariable = isVariable;
            ProjectFile = projectFile;
        }

        public string Name { get; }

        public string Value { get; }

        public bool IsVariable { get; }


        public IProjectFile ProjectFile { get; }
    }
}