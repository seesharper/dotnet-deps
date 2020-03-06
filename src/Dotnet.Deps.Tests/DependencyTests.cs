using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public partial class UnitTest1
    {
        [Fact]
        public void ShouldListOutdatedDependency()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHavePackageReference("LightInject", "5.1.0");
        }

        [Fact]
        public void ShouldUpdateToLatestVersion()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("update");
            result.ProjectFile.ShouldHavePackageReferenceWithLatestVersion("LightInject", "5.1.0");
        }

        [Fact]
        public void ShouldListFloatingDependency()
        {
            var result = new MsBuildTestCase()
               .AddPackage("LightInject", "6.*")
               .Execute();
        }


        [Fact]
        public void ShouldHandleInvalidVersionNumber()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "Rubbish")
                .Execute();
            result.StandardOut.Should().Contain("Warning");
        }

        [Fact]
        public void ShouldIgnorePackageWithMissingVersionNumber()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject")
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldHandleVersionNumberAsVariable()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "$(LightInjectVersion)")
                .AddProperty("LightInjectVersion", "5.1.0")
                .Execute();

            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldListPackagesFromPropsFile()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject")
                .Execute();
        }
    }
}
