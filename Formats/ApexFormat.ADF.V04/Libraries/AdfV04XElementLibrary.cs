using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Libraries;
using RustyOptions;

namespace ApexFormat.ADF.V04.Libraries;

public static class AdfV04XElementLibrary
{
    public static Result<AdfV04Type, Exception> GetAdfV04Type(this XElement xe, AdfV04Type[] types)
    {
        if (!xe.GetAttribute("typeHash").IsSome(out var xTypeHash))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException("Missing type hash attribute"));
        }

        if (!uint.TryParse(xTypeHash, out var typeHash))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException($"Failed to cast type hash '{xTypeHash}' to uint"));
        }
        
        var optionAdfType = types.FirstOrNone(t => t.TypeHash == typeHash);
        if (!optionAdfType.IsSome(out var adfType))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException($"{xe.Name.LocalName} type was missing from type list"));
        }

        return Result.OkExn(adfType);
    }
    
    public static EAdfV04Type GetAdfV04Type(this XElement xe)
    {
        var result = xe.Name.LocalName.ToEAdfV04Type();
        if (result != EAdfV04Type.Scalar)
            return result;
        
        // double check
        if (xe.GetAttribute("type").IsSome(out var xType))
        {
            if (AdfV04TypeEnumLibrary.XNameMap.Values.Reverse().FirstOrNone(n => xType.Contains(n, StringComparison.CurrentCultureIgnoreCase)).IsSome(out var stringType))
            {
                return stringType.ToEAdfV04Type();
            }
            
            return xType.ToEAdfV04Type();
        }

        return result;
    }
}
