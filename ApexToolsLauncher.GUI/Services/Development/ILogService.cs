using System.Runtime.CompilerServices;

namespace ApexToolsLauncher.GUI.Services.Development;

public interface ILogService
{
    void Log(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    );
    void Debug(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    );
    void Info(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    );
    void Warning(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    );
    void Error(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    );
}