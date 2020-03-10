namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a class that is capable of loading an <see cref="IProjectFile"/>.
    /// </summary>
    public interface IProjectLoader
    {
        /// <summary>
        /// Loads an <see cref="IProjectFile"/> from the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path for which to load the project file.</param>
        /// <returns><see cref="IProjectFile"/></returns>
        IProjectFile<NuGetPackageReference> Load(string path);

        /// <summary>
        /// Gets the file extension for the type of projects to be loaded.
        /// </summary>
        string FileExtensions { get; }
    }

}