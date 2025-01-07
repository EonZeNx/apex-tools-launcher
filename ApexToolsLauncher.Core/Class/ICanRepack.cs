namespace ApexToolsLauncher.Core.Class;

public interface ICanRepackPath
{
    bool CanRepackPath(string path);
}

public interface ICanRepackStream
{
    bool CanRepackStream(Stream stream);
}
