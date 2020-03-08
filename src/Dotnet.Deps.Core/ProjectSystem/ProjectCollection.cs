using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class ProjectCollection
    {
        private const string PropertyNameMatcher = @"\$\((.*)\)";


        public ProjectCollection(IProjectFile[] projectFiles)
        {
            ProjectFiles = projectFiles;
        }

        public IProjectFile[] ProjectFiles { get; }

        public Property EvaluateVariable(string name)
        {
            var propertyName = Regex.Match(name, PropertyNameMatcher).Groups[1].Value;
            var property = ProjectFiles.SelectMany(pf => pf.Properties).FirstOrDefault(p => p.Name == propertyName);
            if (property == null)
            {
                throw new InvalidOperationException("Property not found " + name);
            }

            if (property.IsVariable)
            {
                return EvaluateVariable(property.Value);
            }
            else
            {
                return property;
            }

        }
    }
}