using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a NuGet package reference
    /// </summary>
    public abstract class NuGetPackageReference
    {
        public NuGetPackageReference(string name, string version, FloatRange floatRange, bool locked = false)
        {
            Name = name.Trim();
            Version = version.Trim();
            FloatRange = floatRange;
            Locked = locked;
        }

        public string Name { get; }
        public string Version { get; }
        public FloatRange FloatRange { get; }
        public bool Locked { get; }
        public abstract void Update(string newVersion);
    }

}