using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace ApexToolsLauncher.GUI.Services.Development;

public class LogService : ILogService
{
    protected IJSRuntime? JsRuntime { get; set; }
    
    public LogService(IJSRuntime? jsRuntime = null)
    {
        JsRuntime = jsRuntime;
    }

    public async void Log(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "FUNCTION",
        [CallerLineNumber] int lineNumber = -1
    ) {
        if (JsRuntime is null)
        {
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        await JsRuntime.InvokeVoidAsync("console.log", [$"[LOG] {message} | {fileName}:{functionName} line {lineNumber}"]);
    }

    public async void Debug(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "UNKNOWN",
        [CallerLineNumber] int lineNumber = -1
    ) {
        if (JsRuntime is null)
        {
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        await JsRuntime.InvokeVoidAsync("console.debug", [$"[DEBUG] {message} | {fileName}:{functionName} line {lineNumber}"]);
    }

    public async void Info(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "UNKNOWN",
        [CallerLineNumber] int lineNumber = -1
    ) {
        if (JsRuntime is null)
        {
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        await JsRuntime.InvokeVoidAsync("console.info", [$"[INFO] {message} | {fileName}:{functionName} line {lineNumber}"]);
    }

    public async void Warning(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "UNKNOWN",
        [CallerLineNumber] int lineNumber = -1
    ) {
        if (JsRuntime is null)
        {
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        await JsRuntime.InvokeVoidAsync("console.warn", [$"[WARNING] {message} | {fileName}:{functionName} line {lineNumber}"]);
    }

    public async void Error(
        string message,
        [CallerFilePath] string filePath = "FILE",
        [CallerMemberName] string functionName = "UNKNOWN",
        [CallerLineNumber] int lineNumber = -1
    ) {
        if (JsRuntime is null)
        {
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        await JsRuntime.InvokeVoidAsync("console.error", [$"[ERROR] {message} | {fileName}:{functionName} line {lineNumber}"]);
    }
}