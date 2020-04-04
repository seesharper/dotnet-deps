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

        public TestCase AddPackage(string name, string version = "")
        {
            packageReferences.Add((name, version));
            return this;
        }



        protected abstract string CreateProjectFile();

    }

    public class MsBuildTestCase
    {
        private const string msBuildProjectFile = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>";


        protected List<(string name, string version)> packageReferences = new List<(string name, string version)>();

        protected string filter;

        public MsBuildTestCase AddPackage(string name, string version = "")
        {
            packageReferences.Add((name, version));
            return this;
        }

        public MsBuildTestCase WithFilter(string filter)
        {
            this.filter = filter;
            return this;
        }

        protected string CreateProjectFile()
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

        public MsBuildTestResult Execute(params string[] args)
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

                if (!string.IsNullOrEmpty(filter))
                {
                    allArgs.Add("--filter");
                    allArgs.Add(filter);
                }

                var projectFileContent = CreateProjectFile();
                var pathToProjectFile = Path.Combine(projectFolder.Path, "project.csproj");
                File.WriteAllText(pathToProjectFile, projectFileContent);
                int exitCode = app.Execute(allArgs.ToArray());
                return new MsBuildTestResult(XDocument.Load(pathToProjectFile), stdOut.ToString(), stdErr.ToString(), exitCode);
            }
        }
    }


    public class NuspecTestCase
    {
        private const string msBuildProjectFile = @"<?xml version=""1.0""?>
<package >
    <metadata>
    <dependencies>
    </dependencies>
    </metadata>
</package>";

        protected List<(string name, string version)> packageReferences = new List<(string name, string version)>();

        protected string filter;

        public NuspecTestCase AddPackage(string name, string version = "")
        {
            packageReferences.Add((name, version));
            return this;
        }

        public NuspecTestCase WithFilter(string filter)
        {
            this.filter = filter;
            return this;
        }

        protected string CreateProjectFile()
        {
            XDocument projectFile = XDocument.Parse(msBuildProjectFile);
            var itemGroupElement = projectFile.Descendants("dependencies").Single();
            foreach (var packageReference in packageReferences)
            {
                if (string.IsNullOrWhiteSpace(packageReference.version))
                {
                    var packageElement = new XElement("dependency", new XAttribute("id", packageReference.name));
                    itemGroupElement.Add(packageElement);
                }
                else
                {
                    var packageElement = new XElement("dependency", new XAttribute("id", packageReference.name), new XAttribute("version", packageReference.version));
                    itemGroupElement.Add(packageElement);
                }
            }

            return projectFile.ToString();
        }

        public MsBuildTestResult Execute(params string[] args)
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

                if (!string.IsNullOrEmpty(filter))
                {
                    allArgs.Add("--filter");
                    allArgs.Add(filter);
                }

                var projectFileContent = CreateProjectFile();
                var pathToProjectFile = Path.Combine(projectFolder.Path, "project.nuspec");
                File.WriteAllText(pathToProjectFile, projectFileContent);
                int exitCode = app.Execute(allArgs.ToArray());
                return new MsBuildTestResult(XDocument.Load(pathToProjectFile), stdOut.ToString(), stdErr.ToString(), exitCode);
            }
        }
    }



    public class ScriptTestCase
    {
        protected List<(string name, string version)> packageReferences = new List<(string name, string version)>();

        public ScriptTestCase AddPackage(string name, string version = "")
        {
            packageReferences.Add((name, version));
            return this;
        }

        protected string CreateProjectFile()
        {
            var scriptFile = new StringBuilder();
            foreach (var packageReference in packageReferences)
            {
                scriptFile.AppendLine($"#r \"nuget: {packageReference.name}, {packageReference.version}\"");
            }

            return scriptFile.ToString();
        }


        public ScriptTestResult Execute(params string[] args)
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
                var pathToProjectFile = Path.Combine(projectFolder.Path, "script.csx");
                File.WriteAllText(pathToProjectFile, projectFileContent);
                int exitCode = app.Execute(allArgs.ToArray());
                return new ScriptTestResult(File.ReadAllText(pathToProjectFile), stdOut.ToString(), stdErr.ToString(), exitCode);
            }
        }
    }

}
