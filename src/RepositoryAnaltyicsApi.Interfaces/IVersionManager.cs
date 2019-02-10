namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IVersionManager
    {
        string GetPaddedVersion(string version);
        string GetMinorVersion(string version);
    }
}
