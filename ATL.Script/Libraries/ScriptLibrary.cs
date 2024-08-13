using ATL.Script.Variables;

namespace ATL.Script.Libraries;

public static class ScriptLibrary
{
    public static string InterpolateString(string rawString, Dictionary<string, IScriptVariable> variables)
    {
        var interpolated = rawString;
        foreach (var key in variables.Keys)
        {
            var keyVar = $"{ScriptConstantsLibrary.VariableSymbol}{key}";
            var keyValueVar = variables[key];

            if (keyValueVar.MetaType != EScriptVariableMetaType.Normal)
                continue;
            
            var optionKeyValueVar = keyValueVar.As<string>();
            if (!optionKeyValueVar.IsSome(out var keyVarValue))
                continue;

            interpolated = interpolated.Replace(keyVar, keyVarValue);
        }

        return interpolated;
    }

    public static string StripSymbol(string rawString)
    {
        return rawString.Replace(ScriptConstantsLibrary.VariableSymbol, "");
    }
}