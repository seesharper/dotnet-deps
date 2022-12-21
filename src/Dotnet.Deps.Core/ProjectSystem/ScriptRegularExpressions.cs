namespace Dotnet.Deps.Core.ProjectSystem;

public static class ScriptRegularExpressions
{
    const string Hws = @"[\x20\t]*"; // hws = horizontal whitespace

    const string NuGetPattern = @"nuget:"
                              // https://github.com/NuGet/docs.microsoft.com-nuget/issues/543#issue-270039223
                              + Hws + @"(\w+(?:[_.-]\w+)*)"
                              + @"(?:" + Hws + "," + Hws + @"(.+?))?";

    const string WholeNuGetPattern = @"^" + NuGetPattern + @"$";

    const string NuGetDirectivePatternSuffix = Hws + @"""" + NuGetPattern + @"""";

    const string DirectivePatternPrefix = @"^" + Hws + @"#";

    public const string ReferenceDirectivePattern = DirectivePatternPrefix + "r" + NuGetDirectivePatternSuffix;
    public const string LoadDirectivePattern = DirectivePatternPrefix + "load" + NuGetDirectivePatternSuffix;
}
