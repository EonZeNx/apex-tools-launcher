namespace ApexToolsLauncher.Core.Class;

public interface ICanExtractPath
{
    bool CanExtractPath(string path);
}

public interface ICanExtractStream
{
    bool CanExtractStream(Stream stream);
}
