using System.IO;
using System.Text.RegularExpressions;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ScriptProjectLoader : IProjectLoader
    {
        const string Hws = @"[\x20\t]*"; // hws = horizontal whitespace

        const string DirectivePatternPrefix = @"^"
                                            + Hws + @"#";
        const string DirectivePatternSuffix = Hws + @"""nuget:"
                                            // https://github.com/NuGet/docs.microsoft.com-nuget/issues/543#issue-270039223
                                            + Hws + @"(\w+(?:[_.-]\w+)*)"
                                            + @"(?:" + Hws + "," + Hws + @"(.+?))?""";

        public string FileExtensions { get => "csx"; }

        public IProjectFile<NuGetPackageReference> Load(string path)
        {
            var content = File.ReadAllText(path);

            return null;
        }
    }
}