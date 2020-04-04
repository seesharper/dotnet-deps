using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public partial class NuspecTests
    {
        [Fact]
        public void ShouldListOutdatedDependency()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHaveNuspecPackageReference("LightInject", "5.1.0");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldUpdateToLatestVersion()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--update");
            result.ProjectFile.ShouldHaveNuspecPackageReferenceWithLatestVersion("LightInject", "5.1.0");
        }

        [Fact]
        public void ShouldListFloatingDependency()
        {
            var result = new NuspecTestCase()
               .AddPackage("LightInject", "6.*")
               .Execute();
        }


        [Fact]
        public void ShouldHandleInvalidVersionNumber()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject", "Rubbish")
                .Execute();
            result.StandardOut.Should().Contain("Warning");
        }

        [Fact]
        public void ShouldIgnorePackageWithMissingVersionNumber()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject")
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldExcludeFilteredPackages()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithFilter("Microsoft")
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldIncludeFilteredPackages()
        {
            var result = new NuspecTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithFilter("LightInject")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
        }
    }
}
