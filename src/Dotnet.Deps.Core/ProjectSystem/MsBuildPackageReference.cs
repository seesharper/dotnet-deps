using System.Xml.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.ProjectSystem
{
    /// <summary>
    /// Represents a
    /// </summary>
    public class MsBuildPackageReference : NuGetPackageReference
    {
        private readonly XElement packageElement;

        public MsBuildPackageReference(string name, string versionString, NuGetVersion nuGetVersion, NuGetVersionFloatBehavior floatBehavior, XElement packageElement) : base(name, versionString, nuGetVersion, floatBehavior)
        {
            this.packageElement = packageElement;
        }

        public override void Update(string newVersion)
        {
            packageElement.Attribute("Version").Value = newVersion;
        }
    }

}