using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dotnet.Deps.Core;

namespace Dotnet.Deps.Tests
{
    public partial class UnitTest1
    {
        public class TestCase
        {
            private List<(string name, string version)> packageReferences = new List<(string name, string version)>();

            public TestCase AddPackage(string name, string version = "")
            {
                packageReferences.Add((name, version));
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

            private string CreateProjectFile()
            {
                XDocument projectFile = XDocument.Parse(Templates.MsBuildProjectFile);
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
    }
}
