using System.Xml.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class MsBuildPackageReference : NuGetPackageReference
    {
        private readonly XElement packageElement;

        public MsBuildPackageReference(string name, string versionString, bool locked, FloatRange floatRange, XElement packageElement) : base(name, versionString, floatRange, locked)
        {
            this.packageElement = packageElement;
        }

        public override void Update(string newVersion)
        {
            packageElement.Attribute("Version").Value = newVersion;
        }
    }

}