using System.Xml.Linq;

namespace Dotnet.Deps.Tests
{

    public abstract class TestResult
    {
        public TestResult(string standardOut, string standardError, int exitCode)
        {

            StandardOut = standardOut;
            StandardError = standardError;
            ExitCode = exitCode;
        }


        public string StandardOut { get; }
        public string StandardError { get; }
        public int ExitCode { get; }
    }


    public class MsBuildTestResult : TestResult
    {
        public MsBuildTestResult(XDocument projectFile, string standardOut, string standardError, int exitCode) : base(standardOut, standardError, exitCode)
        {
            ProjectFile = projectFile;
        }

        public XDocument ProjectFile { get; }
    }


    public class ScriptTestResult : TestResult
    {
        public ScriptTestResult(string modifiedContent, string standardOut, string standardError, int exitCode) : base(standardOut, standardError, exitCode)
        {
            ModifiedContent = modifiedContent;
        }

        public string ModifiedContent { get; }
    }
}
