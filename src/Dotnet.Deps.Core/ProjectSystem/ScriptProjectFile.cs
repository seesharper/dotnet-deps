using System.IO;
using Dotnet.Deps.Core.ProjectSystem;

namespace Dotnet.Deps.Core
{
    public class ScriptProjectFile : IProjectFile<ScriptPackageReference>
    {
        private readonly string content;

        public ScriptProjectFile(string content, string path)
        {
            this.content = content;
            Path = path;
        }

        public ScriptPackageReference[] PackageReferences { get; set; }

        public string Path { get; }

        public void Save()
        {
            File.WriteAllText(Path, content);
        }
    }
}