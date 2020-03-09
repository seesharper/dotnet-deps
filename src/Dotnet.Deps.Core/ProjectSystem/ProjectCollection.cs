using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ProjectCollection
    {
        public ProjectCollection(IProjectFile[] projectFiles)
        {
            ProjectFiles = projectFiles;
        }

        public IProjectFile[] ProjectFiles { get; }

    }
}