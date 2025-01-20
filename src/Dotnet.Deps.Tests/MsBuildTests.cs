using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public class MsBuildTests
    {
        [Fact]
        public void ShouldListOutdatedDependency()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHaveMsBuildPackageReference("LightInject", "5.1.0");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldUpdateToLatestVersion()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--update");
            result.ProjectFile.ShouldHaveMsBuildPackageReferenceWithLatestVersion("LightInject", "5.1.0");
            result.ExitCode.Should().Be(0);
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
        public void ShouldExcludeFilteredPackages()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithFilter("Microsoft")
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldIncludeFilteredPackages()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithFilter("LightInject")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
        }

        [Fact]
        public void ShouldHandleWhiteSpaceInPackageReferenceName()
        {
            var result = new MsBuildTestCase()
                .AddPackage(" LightInject", "5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHaveMsBuildPackageReference("LightInject", "5.1.0");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldHandleWhiteSpaceInPackageReferenceVersion()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", " 5.1.0")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ProjectFile.ShouldHaveMsBuildPackageReference("LightInject", "5.1.0");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldIgnoreLockedDependency()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0", true)
                .Execute();
            result.StandardOut.Should().NotContain("LightInject 5.1.0 =>");
            result.StandardOut.Should().Contain("LightInject 5.1.0 LOCKED 🔒");
        }
    }
}
