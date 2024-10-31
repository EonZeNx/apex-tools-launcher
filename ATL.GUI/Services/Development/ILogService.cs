namespace ATL.GUI.Services.Development;

public interface ILogService
{
    void Log(string message, string filePath = "FILE", string functionName = "FUNCTION", int lineNumber = -1);
    void Debug(string message, string filePath = "FILE", string functionName = "FUNCTION", int lineNumber = -1);
    void Info(string message, string filePath = "FILE", string functionName = "FUNCTION", int lineNumber = -1);
    void Warning(string message, string filePath = "FILE", string functionName = "FUNCTION", int lineNumber = -1);
    void Error(string message, string filePath = "FILE", string functionName = "FUNCTION", int lineNumber = -1);
}