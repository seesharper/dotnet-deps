using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ProjectCollectionLoader : IProjectCollectionLoader
    {
        private readonly AppConsole console;
        private readonly IProjectLoader[] projectLoaders;

        public ProjectCollectionLoader(AppConsole console, IProjectLoader[] projectLoaders)
        {
            this.console = console;
            this.projectLoaders = projectLoaders;
        }

        public ProjectCollectionLoader(AppConsole console) : this(console, new IProjectLoader[] { new MsBuildProjectLoader(console), new ScriptProjectLoader(console), new NuspecProjectLoader(console) })
        {
        }

        public ProjectCollection Load(string workingDirectory)
        {
            var result = new List<IProjectFile<NuGetPackageReference>>();

            foreach (var projectLoader in projectLoaders)
            {
                var fileExtensions = projectLoader.FileExtensions.Split(';');
                foreach (var fileExtension in fileExtensions)
                {
                    var projectFiles = Directory.GetFiles(workingDirectory, $"*.{fileExtension}", SearchOption.AllDirectories);
                    foreach (var projectFile in projectFiles)
                    {
                        console.WriteNormal($"Analyzing {projectFile}");
                        try
                        {
                            var project = projectLoader.Load(projectFile);
                            if (project.PackageReferences.Length > 0)
                            {
                                result.Add(project);
                            }
                        }
                        catch (System.Exception)
                        {
                            console.WriteNormal($"Unable to read {projectFile}");
                        }
                    }
                }

            }


            return new ProjectCollection(result.ToArray());
        }
    }

}