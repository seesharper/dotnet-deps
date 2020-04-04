using System.Xml.Linq;
using NuGet.Versioning;

namespace Dotnet.Deps.Core.ProjectSystem
{
    public class NuspecPackageReference : NuGetPackageReference
    {
        private readonly XElement packageElement;

        public NuspecPackageReference(string name, string versionString, FloatRange floatRange, XElement packageElement) : base(name, versionString, floatRange)
        {
            this.packageElement = packageElement;
        }

        public override void Update(string newVersion)
        {
            packageElement.Attribute("version").Value = newVersion;
        }
    }
}