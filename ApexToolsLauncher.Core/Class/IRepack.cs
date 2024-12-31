namespace ApexToolsLauncher.Core.Class;

public interface IRepackPathToStream
{
    int RepackPathToStream(string inPath, Stream outStream);
}

public interface IRepackStreamToStream
{
    int RepackStreamToStream(Stream inStream, Stream outStream);
}

public interface IRepackStreamToPath
{
    int RepackStreamToPath(Stream inStream, string outPath);
}

public interface IRepackPathToPath
{
    int RepackPathToPath(string inPath, string outPath);
}
