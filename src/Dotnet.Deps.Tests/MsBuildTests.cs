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

        [Fact]
        public void ShouldIgnoreVersionsNewerThanMinimumAge()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--min-age", "100000");
            result.StandardOut.Should().Contain("Ignoring package versions published less than 100000 days ago ⏳");
            result.StandardOut.Should().Contain("LightInject 5.1.0 - no version is older than 100000 days ⏳");
            result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void ShouldIgnoreVersionsNewerThanMinimumAgeGivenInHours()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--min-age", "12h");
            result.StandardOut.Should().Contain("Ignoring package versions published less than 12 hours ago ⏳");
        }

        [Fact]
        public void ShouldSupportShortFormMinimumAgeOption()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("-ma", "100000d");
            result.StandardOut.Should().Contain("LightInject 5.1.0 - no version is older than 100000 days ⏳");
        }

        [Fact]
        public void ShouldListOutdatedDependencyOlderThanMinimumAge()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--min-age", "1h");
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldUseMinimumAgeFromProjectFile()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithMinimumPackageAge("100000d")
                .Execute();
            result.StandardOut.Should().Contain("LightInject 5.1.0 - no version is older than 100000 days ⏳");
            result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void ShouldPreferMinimumAgeOptionOverProjectFile()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithMinimumPackageAge("100000d")
                .Execute("--min-age", "1h");
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
            result.ExitCode.Should().Be(0xbad);
        }

        [Fact]
        public void ShouldHandleInvalidMinimumAgeOption()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .Execute("--min-age", "rubbish");
            result.StandardOut.Should().Contain("Invalid value 'rubbish' for the --min-age option");
            result.ExitCode.Should().Be(1);
        }

        [Fact]
        public void ShouldHandleInvalidMinimumAgeInProjectFile()
        {
            var result = new MsBuildTestCase()
                .AddPackage("LightInject", "5.1.0")
                .WithMinimumPackageAge("rubbish")
                .Execute();
            result.StandardOut.Should().Contain("invalid PackagesMinimumAge value 'rubbish'");
            result.StandardOut.Should().Contain("LightInject 5.1.0 =>");
        }
    }
}
