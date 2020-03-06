namespace Dotnet.Deps.Core.ProjectSystem
{

    public class PackageReference
    {
        public PackageReference(string name, string version, bool usesVariable)
        {
            Name = name;
            Version = version;
            UsesVariable = usesVariable;
        }

        public string Name { get; }
        public string Version { get; }
        public bool UsesVariable { get; }
    }
}