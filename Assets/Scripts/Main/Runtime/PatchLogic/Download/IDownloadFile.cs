namespace Main.HotUpdate
{
    public interface IDownloadFile
    {
        string Name { get; }

        string Hash { get; }

        long FileSize { get; }

        object UserData { get; }
    }
}