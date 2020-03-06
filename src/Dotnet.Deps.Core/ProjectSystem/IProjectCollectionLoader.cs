namespace Dotnet.Deps.Core.ProjectSystem
{

    public interface IProjectCollectionLoader
    {
        ProjectCollection Load(string path);
    }

}