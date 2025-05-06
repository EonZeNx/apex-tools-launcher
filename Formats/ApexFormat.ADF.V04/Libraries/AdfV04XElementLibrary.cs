using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexToolsLauncher.Core.Libraries;
using RustyOptions;

namespace ApexFormat.ADF.V04.Libraries;

public static class AdfV04XElementLibrary
{
    public static Result<AdfV04Type, Exception> GetAdfV04Type(this XElement xe, AdfV04Type[] types)
    {
        var optionXType = xe.GetAttribute("type");
        if (!optionXType.IsSome(out var xType))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException($"{xe.Name.LocalName} type attribute was missing from struct"));
        }
        
        var optionAdfType = types.FirstOrNone(t => t.Name.Equals(xType, StringComparison.InvariantCultureIgnoreCase));
        if (!optionAdfType.IsSome(out var adfType))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException($"{xe.Name.LocalName} type was missing from type list"));
        }

        return Result.OkExn(adfType);
    }
}
