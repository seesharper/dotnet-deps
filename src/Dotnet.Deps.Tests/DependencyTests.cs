using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public partial class UnitTest1
    {
        [Fact]
        public void ShouldListOutdatedDependency()
        {
            var result = new TestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHavePackageReference("LightInject", "5.1.0");
        }

        [Fact]
        public void ShouldUpdateToLatestVersion()
        {
            var result = new TestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("update");
            result.ProjectFile.ShouldHavePackageReferenceWithLatestVersion("LightInject", "5.1.0");
        }

        [Fact]
        public void ShouldHandleInvalidVersionNumber()
        {
            var result = new TestCase()
                .AddPackage("LightInject", "Rubbish")
                .Execute();
            result.StandardOut.Should().Contain("Warning");
        }

        [Fact]
        public void ShouldIgnorePackageWithMissingVersionNumber()
        {
            var result = new TestCase()
                .AddPackage("LightInject")
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
        }
    }
}
