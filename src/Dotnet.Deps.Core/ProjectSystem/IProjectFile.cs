using System;

namespace Dotnet.Deps.Core.ProjectSystem
{
    /// <summary>
    /// Represents a project file containing NuGet package references.
    /// </summary>
    public interface IProjectFile<out TPackageReference> where TPackageReference : NuGetPackageReference
    {
        /// <summary>
        /// Gets a list of package references in this project file.
        /// </summary>
        /// <value></value>
        TPackageReference[] PackageReferences { get; }

        /// <summary>
        /// Gets the minimum age a package version must have before it is considered for an update
        /// or <c>null</c> if the project file does not specify it.
        /// </summary>
        TimeSpan? MinimumPackageAge { get; }

        /// <summary>
        /// Saves the project file.
        /// </summary>
        void Save();

        string Path { get; }
    }

}