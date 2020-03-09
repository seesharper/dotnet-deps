namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a project file containing NuGet package references.
    /// </summary>
    public interface IProjectFile
    {
        /// <summary>
        /// Gets a list of package references in this project file.
        /// </summary>
        /// <value></value>
        NuGetPackageReference[] PackageReferences { get; }

        /// <summary>
        /// Saves the project file.
        /// </summary>
        void Save();

        string Path { get; }
    }

}