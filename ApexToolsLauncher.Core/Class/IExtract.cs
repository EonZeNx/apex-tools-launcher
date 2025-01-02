using RustyOptions;

namespace ApexToolsLauncher.Core.Class;

public interface IExtractPathToStream
{
    Result<int, Exception> ExtractPathToStream(string inPath, Stream outStream);
}

public interface IExtractStreamToStream
{
    Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream);
}

public interface IExtractStreamToPath
{
    Result<int, Exception> ExtractStreamToPath(Stream inStream, string outPath);
}

public interface IExtractPathToPath
{
    Result<int, Exception> ExtractPathToPath(string inPath, string outPath);
}
