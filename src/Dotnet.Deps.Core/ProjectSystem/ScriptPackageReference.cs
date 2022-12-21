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
            // https://stackoverflow.com/questions/8618557/why-doesnt-in-net-multiline-regular-expressions-match-crlf
            referenceDirectivePattern = $@"^(\s*#r\s*""nuget:\s*)({name})(,\s*)(.*)(\s*""\r?$)";
            loadDirectivePattern = $@"^(\s*#load\s*""nuget:\s*)({name})(,\s*)(.*)(\s*""\r?$)";
            this.content = content;
        }

        public override void Update(string newVersion)
        {
            var match = Regex.Match(content.SourceCode, ScriptRegularExpressions.ReferenceDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var oldMatch = Regex.Match(content.SourceCode, referenceDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            content.SourceCode = Regex.Replace(content.SourceCode, referenceDirectivePattern, "${1}${2}${3}" + newVersion + "${5}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            content.SourceCode = Regex.Replace(content.SourceCode, loadDirectivePattern, "${1}${2}${3}" + newVersion + "${5}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }
    }
}