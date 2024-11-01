using System;

namespace ApexToolsLauncher.CLI.Script.Libraries;

public class ScriptProcessResult(
    EScriptProcessResultType resultType = EScriptProcessResultType.Complete,
    string message = "Ok"
) : ICloneable
{
    public EScriptProcessResultType ResultType = resultType;
    public string Message = message;
    

    public object Clone()
    {
        var result = new ScriptProcessResult
        {
            ResultType = ResultType,
            Message = Message
        };

        return result;
    }
    
    public void Copy(ScriptProcessResult other)
    {
        other.ResultType = ResultType;
        other.Message = Message;
    }
    
    public static ScriptProcessResult Error(string message) =>    new(EScriptProcessResultType.Error, message);
    public static ScriptProcessResult Warning(string message) =>  new(EScriptProcessResultType.Warning, message);
    public static ScriptProcessResult Complete(string message) => new(EScriptProcessResultType.Complete, message);
    public static ScriptProcessResult Info(string message) =>     new(EScriptProcessResultType.Info, message);
    public static ScriptProcessResult Break(string message) =>    new(EScriptProcessResultType.Break, message);
    
    public static ScriptProcessResult Ok() => Complete("Ok");
    public static ScriptProcessResult OkBreak() => Break("Ok");
}
