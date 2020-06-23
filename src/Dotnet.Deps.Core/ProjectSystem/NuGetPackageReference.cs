using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a NuGet package reference
    /// </summary>
    public abstract class NuGetPackageReference
    {
        public NuGetPackageReference(string name, string version, FloatRange floatRange)
        {
            Name = name.Trim();
            Version = version.Trim();
            FloatRange = floatRange;
        }

        public string Name { get; }
        public string Version { get; }
        public FloatRange FloatRange { get; }

        public abstract void Update(string newVersion);
    }

}