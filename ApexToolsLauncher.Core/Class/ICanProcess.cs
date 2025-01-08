namespace ApexToolsLauncher.Core.Class;

public interface ICanProcessStream
{
    static abstract bool CanProcess(Stream stream);
}

public interface ICanProcessPath
{
    static abstract bool CanProcess(string path);
}