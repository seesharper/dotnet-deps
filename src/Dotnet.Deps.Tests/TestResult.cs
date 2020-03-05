using System.Xml.Linq;

namespace Dotnet.Deps.Tests
{
    public partial class UnitTest1
    {
        public class TestResult
        {
            public TestResult(XDocument projectFile, string standardOut, string standardError, int exitCode)
            {
                ProjectFile = projectFile;
                StandardOut = standardOut;
                StandardError = standardError;
                ExitCode = exitCode;
            }

            public XDocument ProjectFile { get; }
            public string StandardOut { get; }
            public string StandardError { get; }
            public int ExitCode { get; }
        }
    }
}
