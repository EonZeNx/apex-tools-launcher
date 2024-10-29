using System.Collections.Generic;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Libraries;

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

            switch (keyValueVar.Type)
            {
            case EScriptVariableType.String:
            {
                var optionValue = keyValueVar.As<string>();
                if (!optionValue.IsSome(out var value))
                    continue;

                interpolated = interpolated.Replace(keyVar, value);
                break;
            }
            case EScriptVariableType.Bool:
            {
                var optionValue = keyValueVar.As<bool>();
                if (!optionValue.IsSome(out var value))
                    continue;

                interpolated = interpolated.Replace(keyVar, value.ToString());
                break;
            }
            case EScriptVariableType.Int:
            {
                var optionValue = keyValueVar.As<int>();
                if (!optionValue.IsSome(out var value))
                    continue;

                interpolated = interpolated.Replace(keyVar, value.ToString());
                break;
            }
            case EScriptVariableType.Float:
            {
                var optionValue = keyValueVar.As<float>();
                if (!optionValue.IsSome(out var value))
                    continue;

                interpolated = interpolated.Replace(keyVar, value.ToString());
                break;
            }
            case EScriptVariableType.Unknown:
            default:
                break;
            }
        }

        return interpolated;
    }

    public static string StripSymbol(string rawString)
    {
        return rawString.Replace(ScriptConstantsLibrary.VariableSymbol, "");
    }
}