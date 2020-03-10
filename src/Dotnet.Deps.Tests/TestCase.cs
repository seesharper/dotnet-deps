using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dotnet.Deps.Core;

namespace Dotnet.Deps.Tests
{

    public abstract class TestCase
    {
        protected List<(string name, string version)> packageReferences = new List<(string name, string version)>();

        private ProjectFileTemplate projectFileTemplate = ProjectFileTemplates.MsBuild;

        public TestCase AddPackage(string name, string version = "")
        {
            packageReferences.Add((name, version));
            return this;
        }

        public TestCase WithTemplate(ProjectFileTemplate projectFileTemplate)
        {
            this.projectFileTemplate = projectFileTemplate;
            return this;
        }

        public TestResult Execute(params string[] args)
        {
            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            var app = new App(new AppConsole(new StringWriter(stdOut), new StringWriter(stdErr)));

            using (var projectFolder = new DisposableFolder())
            {
                List<string> allArgs = new List<string>();
                allArgs.Add("-cwd");
                allArgs.Add(projectFolder.Path);
                allArgs.AddRange(args);

                var projectFileContent = CreateProjectFile();
                var pathToProjectFile = Path.Combine(projectFolder.Path, "project.csproj");
                File.WriteAllText(pathToProjectFile, projectFileContent);
                int exitCode = app.Execute(allArgs.ToArray());
                return new TestResult(XDocument.Load(pathToProjectFile), stdOut.ToString(), stdErr.ToString(), exitCode);
            }
        }

        protected abstract string CreateProjectFile();

    }

    public class MsBuildTestCase : TestCase
    {
        private const string msBuildProjectFile = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>";

        protected override string CreateProjectFile()
        {
            XDocument projectFile = XDocument.Parse(msBuildProjectFile);
            var itemGroupElement = projectFile.Descendants("ItemGroup").Single();
            foreach (var packageReference in packageReferences)
            {
                if (string.IsNullOrWhiteSpace(packageReference.version))
                {
                    var packageElement = new XElement("PackageReference", new XAttribute("Include", packageReference.name));
                    itemGroupElement.Add(packageElement);
                }
                else
                {
                    var packageElement = new XElement("PackageReference", new XAttribute("Include", packageReference.name), new XAttribute("Version", packageReference.version));
                    itemGroupElement.Add(packageElement);
                }
            }

            return projectFile.ToString();
        }
    }

    public class ScriptTestCase : TestCase
    {
        protected override string CreateProjectFile()
        {
            var scriptFile = new StringBuilder();
            foreach (var packageReference in packageReferences)
            {
                scriptFile.AppendLine($"#r \"nuget: {packageReference.name}, {packageReference.version}\"");
            }

            return scriptFile.ToString();
        }
    }

}
