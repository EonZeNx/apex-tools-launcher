namespace ApexToolsLauncher.Core.Class;

public interface IExtractFile
{
    int ExtractFile(Stream inStream, Stream outStream);
}

public interface IExtractDirectory
{
    int ExtractFile(Stream inStream, string outFilePath);
}
