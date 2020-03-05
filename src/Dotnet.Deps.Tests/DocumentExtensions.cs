using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
namespace Dotnet.Deps.Tests
{
    public static class DocumentExtensions
    {
        public static void ShouldHavePackageReference(this XDocument document, string name, string version)
        {
            document.Descendants("PackageReference").Where(e => e.Attribute("Include").Value == name && e.Attribute("Version").Value == version).Should().HaveCount(1);
        }

        public static void ShouldHavePackageReferenceWithLatestVersion(this XDocument document, string name, string version)
        {
            document.Descendants("PackageReference").Where(e => e.Attribute("Include").Value == name && e.Attribute("Version").Value != version).Should().HaveCount(1);
        }
    }
}