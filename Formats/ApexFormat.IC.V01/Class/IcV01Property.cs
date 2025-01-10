using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Variant - <see cref="EIcV01Variant"/>
/// <br/>Data - <see cref="object"/>?
/// </summary>
public class IcV01Property
{
    public uint NameHash = 0;
    public EIcV01Variant Variant = EIcV01Variant.Unassigned;
    public object? Data = null;
}

public static class IcV01PropertyLibrary
{
    public const int SizeOf = sizeof(uint) // NameHash
                              + sizeof(EIcV01Variant) // Variant
                              + sizeof(uint); // Min data size
    
    public const string XName = "value";
    
    public static Option<IcV01Property> ReadIcV01Property(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<IcV01Property>.None;
        }
        
        var result = new IcV01Property
        {
            NameHash = stream.Read<uint>(),
            Variant = stream.Read<EIcV01Variant>(),
        };
        
        switch (result.Variant)
        {
            case EIcV01Variant.Unassigned:
            case EIcV01Variant.UInteger32:
                result.Data = stream.Read<uint>();
                break;
            case EIcV01Variant.Float32:
                result.Data = stream.Read<float>();
                break;
            case EIcV01Variant.String:
                var stringLength = stream.Read<ushort>();
                if (stringLength == 20)
                {
                    ;
                }
                result.Data = stream.ReadStringOfLength(stringLength);
                break;
            case EIcV01Variant.Vector2:
                result.Data = stream.ReadArray<float>(2);
                break;
            case EIcV01Variant.Vector3:
                result.Data = stream.ReadArray<float>(3);
                break;
            case EIcV01Variant.Vector4:
                result.Data = stream.ReadArray<float>(4);
                break;
            case EIcV01Variant.Matrix3X3:
                result.Data = stream.ReadArray<float>(9);
                break;
            case EIcV01Variant.Matrix3X4:
                result.Data = stream.ReadArray<float>(12);
                break;
            case EIcV01Variant.UInteger32Array:
                result.Data = stream.ReadArrayLengthPrefix<uint>();
                break;
            case EIcV01Variant.Float32Array:
                result.Data = stream.ReadArrayLengthPrefix<float>();
                break;
            case EIcV01Variant.ByteArray:
                result.Data = stream.ReadArrayLengthPrefix<byte>();
                break;
            case EIcV01Variant.ObjectId:
                result.Data = stream.ReadIcV01ObjectId();
                break;
            case EIcV01Variant.Events:
                result.Data = stream.ReadArrayLengthPrefix<(uint, uint)>();
                break;
            default:
                break;
        }

        return Option.Some(result);
    }

    public static Option<Exception> Write(this Stream stream, IcV01Property property)
    {
        stream.Write(property.NameHash);
        stream.Write(property.Variant);

        if (property.Data is null)
        {
            return Option.Some<Exception>(new InvalidOperationException("property.Data is null"));
        }

        switch (property.Variant)
        {
            case EIcV01Variant.Unassigned:
            case EIcV01Variant.UInteger32:
                stream.Write((uint) property.Data);
                break;
            case EIcV01Variant.Float32:
                stream.Write((float) property.Data);
                break;
            case EIcV01Variant.String:
                var dataString = (string) property.Data;
                stream.Write((ushort) dataString.Length);
                stream.Write(Encoding.UTF8.GetBytes(dataString));
                break;
            case EIcV01Variant.Vector2:
                var dataVec2 = (float[]) property.Data;
                if (dataVec2.Length != 2)
                    return Option.Some<Exception>(new ArgumentOutOfRangeException($"{property.Variant} had length {dataVec2.Length}"));
                foreach (var v in dataVec2)
                    stream.Write(v);
                break;
            case EIcV01Variant.Vector3:
                var dataVec3 = (float[]) property.Data;
                if (dataVec3.Length != 3)
                    return Option.Some<Exception>(new ArgumentOutOfRangeException($"{property.Variant} had length {dataVec3.Length}"));
                foreach (var v in dataVec3)
                    stream.Write(v);
                break;
            case EIcV01Variant.Vector4:
                var dataVec4 = (float[]) property.Data;
                if (dataVec4.Length != 4)
                    return Option.Some<Exception>(new ArgumentOutOfRangeException($"{property.Variant} had length {dataVec4.Length}"));
                foreach (var v in dataVec4)
                    stream.Write(v);
                break;
            case EIcV01Variant.Matrix3X3:
                var dataMat3 = (float[]) property.Data;
                if (dataMat3.Length != 9)
                    return Option.Some<Exception>(new ArgumentOutOfRangeException($"{property.Variant} had length {dataMat3.Length}"));
                foreach (var v in dataMat3)
                    stream.Write(v);
                break;
            case EIcV01Variant.Matrix3X4:
                var dataMat3X4 = (float[]) property.Data;
                if (dataMat3X4.Length != 12)
                    return Option.Some<Exception>(new ArgumentOutOfRangeException($"{property.Variant} had length {dataMat3X4.Length}"));
                foreach (var v in dataMat3X4)
                    stream.Write(v);
                break;
            case EIcV01Variant.UInteger32Array:
                var dataUintArray = (uint[]) property.Data;
                stream.Write((uint) dataUintArray.Length);
                foreach (var v in dataUintArray)
                    stream.Write(v);
                break;
            case EIcV01Variant.Float32Array:
                var dataFloatArray = (float[]) property.Data;
                stream.Write((uint) dataFloatArray.Length);
                foreach (var v in dataFloatArray)
                    stream.Write(v);
                break;
            case EIcV01Variant.ByteArray:
                var dataByteArray = (byte[]) property.Data;
                stream.Write((uint) dataByteArray.Length);
                foreach (var v in dataByteArray)
                    stream.Write(v);
                break;
            case EIcV01Variant.ObjectId:
                var objectId = (IcV01ObjectId) property.Data;
                stream.Write(objectId);
                break;
            case EIcV01Variant.Events:
                var dataEvents = ((uint, uint)[]) property.Data;
                stream.Write((uint) dataEvents.Length);
                foreach (var evt in dataEvents)
                {
                    stream.Write(evt.Item1);
                    stream.Write(evt.Item2);
                }
                break;
            default:
                return Option.Some<Exception>(new ArgumentOutOfRangeException($"invalid variant {property.Variant}"));
        }
        
        return Option<Exception>.None;
    }
    
    public static XElement ToXElement(this IcV01Property property)
    {
        var xe = new XElement(XName);
        
        var optionHashResult = HashDatabases.Lookup(property.NameHash, EHashType.FilePath);
        if (optionHashResult.IsSome(out var hashResult))
        {
            xe.SetAttributeValue("name", hashResult.Value);
        }
        else
        {
            xe.SetAttributeValue("id", $"{property.NameHash:X8}");
        }
        
        xe.SetAttributeValue("type", property.Variant.ToXName());
        if (property.Data is null)
            return xe;

        if (property.Variant.IsPrimitive())
        {
            switch (property.Variant)
            {
                case EIcV01Variant.Unassigned:
                case EIcV01Variant.UInteger32:
                case EIcV01Variant.Total:
                    xe.SetValue((uint) property.Data);
                    break;
                case EIcV01Variant.Float32:
                    xe.SetValue((float) property.Data);
                    break;
            }

            return xe;
        }
        
        switch (property.Variant)
        {
            case EIcV01Variant.String:
                xe.SetValue(property.Data);
                break;
            case EIcV01Variant.Vector2:
            case EIcV01Variant.Vector3:
            case EIcV01Variant.Vector4:
            case EIcV01Variant.Float32Array:
                var vec = (float[]) property.Data;
                xe.SetValue(string.Join(",", vec));
                break;
            case EIcV01Variant.Matrix3X3:
            case EIcV01Variant.Matrix3X4:
                var mat = (float[]) property.Data;
                xe.SetValue(string.Join(",", mat));
                break;
            case EIcV01Variant.UInteger32Array:
                var ints = (uint[]) property.Data;
                xe.SetValue(string.Join(",", ints));
                break;
            case EIcV01Variant.ByteArray:
                var bytes = (byte[]) property.Data;
                xe.SetValue(string.Join(",", bytes.Select(b => $"{b:X2}")));
                break;
            case EIcV01Variant.ObjectId:
                var oid = (IcV01ObjectId) property.Data;
                var oidString = oid.ToString();
                xe.SetValue(oidString);
                break;
            case EIcV01Variant.Events:
                var eventPairs = ((uint, uint)[]) property.Data;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                xe.SetValue(string.Join(", ", events));
                break;
            case EIcV01Variant.Deprecated:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
    
    public static Result<bool, Exception> FromXElement(this IcV01Property property, XElement xe)
    {
        const StringSplitOptions defaultStringSplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        
        if (!string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<bool>(new System.Xml.XmlException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        var nameHashOption = xe.GetAttributeOrNone("name")
            .Map(s => s.Jenkins())
            .OrElse(() => xe.GetAttributeOrNone("id")
                .Map(s => uint.Parse(s, NumberStyles.HexNumber)));
        
        if (!nameHashOption.IsSome(out var nameHash))
        {
            return Result.Err<bool>(new InvalidOperationException("both name and id attributes are both missing"));
        }

        property.NameHash = nameHash;
        
        if (!xe.GetAttributeOrNone("type").IsSome(out var typeAttribute))
        {
            return Result.Err<bool>(new InvalidOperationException("type attribute missing"));
        }

        property.Variant = IcV01VariantLibrary.FromXName(typeAttribute);
        
        switch (property.Variant)
        {
            case EIcV01Variant.Unassigned:
            case EIcV01Variant.UInteger32:
            {
                if (!uint.TryParse(xe.Value, out var result))
                {
                    return Result.Err<bool>(new XmlSchemaException($"{xe.Value} is not a valid {property.Variant.ToXName()}"));
                }
                
                property.Data = result;
                return Result.OkExn(true);
            }
            case EIcV01Variant.Float32:
            {
                if (!float.TryParse(xe.Value, out var result))
                {
                    return Result.Err<bool>(new XmlSchemaException($"{xe.Value} is not a valid {property.Variant.ToXName()}"));
                }
                
                property.Data = result;
                return Result.OkExn(true);
            }
            case EIcV01Variant.String:
            {
                property.Data = xe.Value;
                return Result.OkExn(true);
            }
            case EIcV01Variant.Vector2:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length == 2
                    ? Array.ConvertAll(strValues, float.Parse) : new float[2];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Vector3:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length == 3
                    ? Array.ConvertAll(strValues, float.Parse) : new float[3];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Vector4:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length == 4
                    ? Array.ConvertAll(strValues, float.Parse) : new float[4];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Float32Array:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length != 0
                    ? Array.ConvertAll(strValues, float.Parse) : [];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Matrix3X3:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length == 9
                    ? Array.ConvertAll(strValues, float.Parse) : new float[9];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Matrix3X4:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length == 12
                    ? Array.ConvertAll(strValues, float.Parse) : new float[12];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.UInteger32Array:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length != 0
                    ? Array.ConvertAll(strValues, uint.Parse) : [];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.ByteArray:
            {
                var strValues = xe.Value.Split(",", defaultStringSplitOptions);
                property.Data = strValues.Length != 0
                    ? Array.ConvertAll(strValues, MathLibrary.HexToByte) : [];
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Deprecated:
            {
                return Result.Err<bool>(new ArgumentOutOfRangeException());
            }
            case EIcV01Variant.ObjectId:
            {
                property.Data = IcV01ObjectIdLibrary.FromString(xe.Value);
                
                return Result.OkExn(true);
            }
            case EIcV01Variant.Events:
            {
                if (string.IsNullOrEmpty(xe.Value))
                {
                    property.Data = Array.Empty<(uint, uint)>();
                    return Result.OkExn(true);
                }

                property.Data = xe.Value.Split(", ", defaultStringSplitOptions)
                    .Select(eStr => eStr.Split("=", defaultStringSplitOptions)
                        .Select(MathLibrary.HexToUInt)
                        .ToArray())
                    .Select(e => (e[0], e[1]))
                    .ToArray();
                
                break;
            }
            case EIcV01Variant.Total:
            {
                return Result.Err<bool>(new ArgumentOutOfRangeException());
            }
            default:
            {
                return Result.Err<bool>(new ArgumentOutOfRangeException());
            }
        }
        
        return Result.OkExn(true);
    }
}