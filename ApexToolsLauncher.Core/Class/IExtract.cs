namespace ApexToolsLauncher.Core.Class;

public interface IExtractPathToStream
{
    int ExtractPathToStream(string inPath, Stream outStream);
}

public interface IExtractStreamToStream
{
    int ExtractStreamToStream(Stream inStream, Stream outStream);
}

public interface IExtractStreamToPath
{
    int ExtractStreamToPath(Stream inStream, string outPath);
}

public interface IExtractPathToPath
{
    int ExtractPathToPath(string inPath, string outPath);
}
