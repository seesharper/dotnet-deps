namespace Dotnet.Deps.Tests
{
    public static class ProjectFileTemplates
    {
        private const string msBuildProjectFile = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>";


        public static readonly ProjectFileTemplate MsBuild = new ProjectFileTemplate(msBuildProjectFile, "csproj");

        public static readonly ProjectFileTemplate Props = new ProjectFileTemplate(msBuildProjectFile, "csproj");


    }

    public class ProjectFileTemplate
    {
        public ProjectFileTemplate(string content, string fileExtensions)
        {
            Content = content;
            FileExtensions = fileExtensions;
        }

        public string Content { get; }

        public string FileExtensions { get; }


    }
}