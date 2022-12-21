using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ScriptProjectLoader : IProjectLoader
    {
        const string Hws = @"[\x20\t]*"; // hws = horizontal whitespace

        const string NuGetPattern = @"nuget:"
                                  // https://github.com/NuGet/docs.microsoft.com-nuget/issues/543#issue-270039223
                                  + Hws + @"(\w+(?:[_.-]\w+)*)"
                                  + @"(?:" + Hws + "," + Hws + @"(.+?))?";

        const string WholeNuGetPattern = @"^" + NuGetPattern + @"$";

        const string NuGetDirectivePatternSuffix = Hws + @"""" + NuGetPattern + @"""";

        const string DirectivePatternPrefix = @"^" + Hws + @"#";

        const string ReferenceDirectivePattern = DirectivePatternPrefix + "r" + NuGetDirectivePatternSuffix;
        const string LoadDirectivePattern = DirectivePatternPrefix + "load" + NuGetDirectivePatternSuffix;


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
            var matches = Regex.Matches(content, ReferenceDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Cast<Match>()
                .Union(Regex.Matches(content, LoadDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Cast<Match>()).ToArray();
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