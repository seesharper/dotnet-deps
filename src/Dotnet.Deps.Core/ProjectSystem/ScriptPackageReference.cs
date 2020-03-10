using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ScriptPackageReference : NuGetPackageReference
    {
        private readonly string content;

        public ScriptPackageReference(string name, string version, FloatRange floatRange, string content) : base(name, version, floatRange)
        {
            this.content = content;
        }

        public override void Update(string newVersion)
        {
            throw new System.NotImplementedException();
        }
    }
}