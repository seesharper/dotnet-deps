using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ProjectCollectionLoader : IProjectCollectionLoader
    {
        private readonly AppConsole console;
        private readonly IProjectLoader projectLoader;

        public ProjectCollectionLoader(AppConsole console, IProjectLoader projectLoader)
        {
            this.console = console;
            this.projectLoader = projectLoader;
        }

        public ProjectCollection Load(string workingDirectory)
        {
            var result = new List<IProjectFile>();

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

                    }
                    //result.Add(projectLoader.Load(projectFile));
                }
            }

            return new ProjectCollection(result.ToArray());
        }
    }

}