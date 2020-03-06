using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dotnet.Deps.Core;

namespace Dotnet.Deps.ProjectSystem
{

    public interface IProjectCollectionLoader
    {
        IProjectFile[] Load(string path);
    }

    public class ProjectCollectionLoader : IProjectCollectionLoader
    {
        private readonly AppConsole console;
        private readonly IProjectLoader projectLoader;

        public ProjectCollectionLoader(AppConsole console, IProjectLoader projectLoader)
        {
            this.console = console;
            this.projectLoader = projectLoader;
        }

        public IProjectFile[] Load(string workingDirectory)
        {
            var projectFiles = Directory.GetFiles(workingDirectory, $"*.{projectLoader.FileExtension}", SearchOption.AllDirectories);
            var result = new List<IProjectFile>();
            foreach (var projectFile in projectFiles)
            {
                console.WriteNormal($"Analyzing {projectFile}");
                result.Add(projectLoader.Load(projectFile));
            }

            return result.ToArray(); ;
        }
    }

}