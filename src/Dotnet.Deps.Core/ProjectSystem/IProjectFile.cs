namespace Dotnet.Deps.ProjectSystem
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
        NuGetPackageReference[] NuGetPackageReferences { get; }

        /// <summary>
        /// Saves the project file.
        /// </summary>
        void Save();
    }

}