using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ScriptPackageReference : NuGetPackageReference
    {
        private readonly string referenceDirectivePattern;

        private readonly string loadDirectivePattern;
        private readonly ScriptFileContent content;

        public ScriptPackageReference(string name, string version, FloatRange floatRange, ScriptFileContent content) : base(name, version, floatRange)
        {
            referenceDirectivePattern = $@"^(\s*#r\s*""nuget:\s*)({name})(,\s*)(.*)(\s*""$)";
            loadDirectivePattern = $@"^(\s*#load\s*""nuget:\s*)({name})(,\s*)(.*)(\s*""$)";
            this.content = content;
        }

        public override void Update(string newVersion)
        {
            content.SourceCode = Regex.Replace(content.SourceCode, referenceDirectivePattern, $"$1$2$3{newVersion}$5");
        }
    }
}