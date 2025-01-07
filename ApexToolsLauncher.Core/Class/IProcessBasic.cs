namespace ApexToolsLauncher.Core.Class;

public interface IProcessBasic
{
    int ProcessBasic(string inFilePath, string outDirectory);
    string GetProcessorName();
}
