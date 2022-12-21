using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ScriptProjectLoader : IProjectLoader
    {
        private readonly AppConsole console;

        public ScriptProjectLoader(AppConsole console)
        {
            this.console = console;
        }

        public string FileExtensions { get => "csx"; }

        public IProjectFile<NuGetPackageReference> Load(string path)
        {
            var content = File.ReadAllText(path);
            var scriptFileContent = new ScriptFileContent() { SourceCode = content };
            var matches = Regex.Matches(content, ScriptRegularExpressions.ReferenceDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Cast<Match>()
                .Union(Regex.Matches(content, ScriptRegularExpressions.LoadDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Cast<Match>()).ToArray();
            var packageReferences = new List<ScriptPackageReference>();
            foreach (var match in matches)
            {
                var packageName = match.Groups[1].Value;
                var packageVersion = match.Groups[2].Value;

                if (FloatRange.TryParse(packageVersion, out var floatRange))
                {
                    var nugetVersion = floatRange.MinVersion;
                    var nugetPackageReference = new ScriptPackageReference(packageName, packageVersion, floatRange, scriptFileContent);
                    packageReferences.Add(nugetPackageReference);
                }
                else
                {
                    console.WriteError($"Warning: The package '{packageName}' has an invalid version number '{packageVersion}'");
                }
            }

            return new ScriptProjectFile(scriptFileContent, path) { PackageReferences = packageReferences.ToArray() };
        }
    }
}