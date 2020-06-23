using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
namespace Dotnet.Deps.Tests
{
    public static class DocumentExtensions
    {
        public static void ShouldHaveMsBuildPackageReference(this XDocument document, string name, string version)
        {
            document.Descendants("PackageReference").Where(e => e.Attribute("Include").Value.Trim() == name && e.Attribute("Version").Value.Trim() == version).Should().HaveCount(1);
        }

        public static void ShouldHaveMsBuildPackageReferenceWithLatestVersion(this XDocument document, string name, string version)
        {
            document.Descendants("PackageReference").Where(e => e.Attribute("Include").Value.Trim() == name && e.Attribute("Version").Value.Trim() != version).Should().HaveCount(1);
        }

        public static void ShouldHaveNuspecPackageReference(this XDocument document, string name, string version)
        {
            document.Descendants("dependency").Where(e => e.Attribute("id").Value.Trim() == name && e.Attribute("version").Value.Trim() == version).Should().HaveCount(1);
        }

        public static void ShouldHaveNuspecPackageReferenceWithLatestVersion(this XDocument document, string name, string version)
        {
            document.Descendants("dependency").Where(e => e.Attribute("id").Value.Trim() == name && e.Attribute("version").Value.Trim() != version).Should().HaveCount(1);
        }
    }
}