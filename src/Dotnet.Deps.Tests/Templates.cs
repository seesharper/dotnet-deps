namespace Dotnet.Deps.Tests
{
    public static class Templates
    {
        public const string MsBuildProjectFile = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>";
    }
}