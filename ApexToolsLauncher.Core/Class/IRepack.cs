using RustyOptions;

namespace ApexToolsLauncher.Core.Class;

public interface IRepackPathToStream
{
    Result<int, Exception> RepackPathToStream(string inPath, Stream outStream);
}

public interface IRepackStreamToStream
{
    Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream);
}

public interface IRepackStreamToPath
{
    Result<int, Exception> RepackStreamToPath(Stream inStream, string outPath);
}

public interface IRepackPathToPath
{
    Result<int, Exception> RepackPathToPath(string inPath, string outPath);
}
