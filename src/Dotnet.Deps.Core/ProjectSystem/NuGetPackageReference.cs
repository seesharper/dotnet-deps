using NuGet.Versioning;

namespace Dotnet.Deps.ProjectSystem
{
    /// <summary>
    /// Represents a NuGet package reference
    /// </summary>
    public abstract class NuGetPackageReference
    {
        public NuGetPackageReference(string name, string versionString, NuGetVersion nuGetVersion, NuGetVersionFloatBehavior floatBehavior)
        {
            Name = name;
            VersionString = versionString;
            NuGetVersion = nuGetVersion;
            FloatBehavior = floatBehavior;
        }

        public string Name { get; }
        public string VersionString { get; }
        public NuGetVersion NuGetVersion { get; }
        public NuGetVersionFloatBehavior FloatBehavior { get; }
        public abstract void Update(string newVersion);
    }

}