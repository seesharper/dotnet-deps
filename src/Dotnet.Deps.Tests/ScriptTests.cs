using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public class ScriptTests
    {
        [Fact]
        public void ShouldListOutdatedDependency()
        {
            var result = new ScriptTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            //result.ProjectFile.ShouldHavePackageReference("LightInject", "5.1.0");
        }
    }
}