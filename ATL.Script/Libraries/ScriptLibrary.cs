using ATL.Script.Variables;

namespace ATL.Script.Libraries;

public static class ScriptLibrary
{
    public static string InterpolateString(string rawString, Dictionary<string, ScriptVariable> variables)
    {
        var interpolated = rawString;
        foreach (var key in variables.Keys)
        {
            var keyVar = $"{ScriptConstantsLibrary.VariableSymbol}{key}";
            var keyValueVar = variables[key];
            
            var optionKeyValueVar = keyValueVar.AsString();
            if (!optionKeyValueVar.IsSome(out var keyVarValue))
                continue;

            interpolated = interpolated.Replace(keyVar, keyVarValue);
        }

        return interpolated;
    }
}