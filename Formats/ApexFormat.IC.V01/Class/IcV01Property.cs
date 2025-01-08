using System.Globalization;
using System.Xml.Linq;
using System.Xml.Schema;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using Microsoft.VisualBasic;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Variant - <see cref="EIcVariantV01"/>
/// <br/>Data - <see cref="object"/>?
/// </summary>
public class IcV01Property
{
    public uint NameHash = 0;
    public EIcVariantV01 Variant = EIcVariantV01.Unassigned;
    public object? Data = null;
}

public static class IcV01PropertyLibrary
{
    public const string XName = "value";
    
    public const int SizeOf = sizeof(uint) // NameHash
                              + sizeof(EIcVariantV01) // Variant
                              + sizeof(uint); // Min data size
    
    public static Option<IcV01Property> ReadIcV01Property(this Stream stream)
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<IcV01Property>.None;
        }
        
        var result = new IcV01Property
        {
            NameHash = stream.Read<uint>(),
            Variant = stream.Read<EIcVariantV01>(),
        };
        
        switch (result.Variant)
        {
            case EIcVariantV01.Unassigned:
            case EIcVariantV01.UInteger32:
                result.Data = stream.Read<uint>();
                break;
            case EIcVariantV01.Float32:
                result.Data = stream.Read<float>();
                break;
            case EIcVariantV01.String:
                var stringLength = stream.Read<ushort>();
                if (stringLength == 20)
                {
                    ;
                }
                result.Data = stream.ReadStringOfLength(stringLength);
                break;
            case EIcVariantV01.Vector2:
                result.Data = stream.ReadArray<float>(2);
                break;
            case EIcVariantV01.Vector3:
                result.Data = stream.ReadArray<float>(3);
                break;
            case EIcVariantV01.Vector4:
                result.Data = stream.ReadArray<float>(4);
                break;
            case EIcVariantV01.Matrix3X3:
                result.Data = stream.ReadArray<float>(9);
                break;
            case EIcVariantV01.Matrix3X4:
                result.Data = stream.ReadArray<float>(12);
                break;
            case EIcVariantV01.UInteger32Array:
                result.Data = stream.ReadArrayLengthPrefix<uint>();
                break;
            case EIcVariantV01.Float32Array:
                result.Data = stream.ReadArrayLengthPrefix<float>();
                break;
            case EIcVariantV01.ByteArray:
                result.Data = stream.ReadArrayLengthPrefix<byte>();
                break;
            case EIcVariantV01.ObjectId:
                result.Data = stream.ReadIcV01ObjectId();
                break;
            case EIcVariantV01.Events:
                result.Data = stream.ReadArrayLengthPrefix<(uint, uint)>();
                break;
            default:
                break;
        }

        return Option.Some(result);
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
        
        xe.SetAttributeValue("type", property.Variant.ToXmlString());
        if (property.Data is null)
            return xe;

        if (property.Variant.IsPrimitive())
        {
            switch (property.Variant)
            {
                case EIcVariantV01.Unassigned:
                case EIcVariantV01.UInteger32:
                case EIcVariantV01.Total:
                    xe.SetValue((uint) property.Data);
                    break;
                case EIcVariantV01.Float32:
                    xe.SetValue((float) property.Data);
                    break;
            }

            return xe;
        }
        
        switch (property.Variant)
        {
            case EIcVariantV01.String:
                xe.SetValue(property.Data);
                break;
            case EIcVariantV01.Vector2:
            case EIcVariantV01.Vector3:
            case EIcVariantV01.Vector4:
            case EIcVariantV01.Float32Array:
                var vec = (float[]) property.Data;
                xe.SetValue(string.Join(",", vec));
                break;
            case EIcVariantV01.Matrix3X3:
            case EIcVariantV01.Matrix3X4:
                var mat = (float[]) property.Data;
                xe.SetValue(string.Join(",", mat));
                break;
            case EIcVariantV01.UInteger32Array:
                var ints = (uint[]) property.Data;
                xe.SetValue(string.Join(",", ints));
                break;
            case EIcVariantV01.ByteArray:
                var bytes = (byte[]) property.Data;
                xe.SetValue(string.Join(",", bytes.Select(b => $"{b:X2}")));
                break;
            case EIcVariantV01.ObjectId:
                var objectIdValue = (IcV01ObjectId) property.Data;
                var objectId = objectIdValue.String();
                xe.SetValue(objectId);
                break;
            case EIcVariantV01.Events:
                var eventPairs = ((uint, uint)[]) property.Data;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                xe.SetValue(string.Join(", ", events));
                break;
            case EIcVariantV01.Deprecated:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
    
    public static Result<bool, Exception> FromXElement(this IcV01Property property, XElement xe)
    {
        if (string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<bool>(new System.Xml.XmlException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        xe.GetAttributeOrNone("name").Match(
            s => property.NameHash = s.Jenkins(),
            () => xe.GetAttributeOrNone("id")
                .MatchSome(s => property.NameHash = uint.Parse(s, NumberStyles.HexNumber)));

        var optionAttribute = xe.GetAttributeOrNone("type");
        if (!optionAttribute.IsSome(out var typeAttribute))
        {
            return Result.Err<bool>(new ArgumentOutOfRangeException());
        }

        property.Variant = typeAttribute.ToEIcVariantV01();
        
        switch (property.Variant)
        {
            case EIcVariantV01.Unassigned:
            case EIcVariantV01.UInteger32:
            {
                if (!uint.TryParse(xe.Value, out var result))
                {
                    return Result.Err<bool>(new XmlSchemaException($"{xe.Value} is not a valid {property.Variant.ToXmlString()}"));
                }
                
                property.Data = result;
                return Result.OkExn(true);
            }
            case EIcVariantV01.Float32:
            {
                if (!float.TryParse(xe.Value, out var result))
                {
                    return Result.Err<bool>(new XmlSchemaException($"{xe.Value} is not a valid {property.Variant.ToXmlString()}"));
                }
                
                property.Data = result;
                return Result.OkExn(true);
            }
            case EIcVariantV01.String:
            {
                property.Data = xe.Value;
                return Result.OkExn(true);
            }
            case EIcVariantV01.Vector2:
            case EIcVariantV01.Vector3:
            case EIcVariantV01.Vector4:
            case EIcVariantV01.Float32Array:
            {
                var strValues = xe.Value.Split(",");
                property.Data = Array.ConvertAll(strValues, float.Parse);
                
                return Result.OkExn(true);
            }
            case EIcVariantV01.Matrix3X3:
            case EIcVariantV01.Matrix3X4:
            {
                var strValues = xe.Value.Split(",");
                property.Data = Array.ConvertAll(strValues, float.Parse);
                
                return Result.OkExn(true);
            }
            case EIcVariantV01.UInteger32Array:
            {
                var strValues = xe.Value.Split(",");
                property.Data = Array.ConvertAll(strValues, uint.Parse);
                
                return Result.OkExn(true);
            }
            case EIcVariantV01.ByteArray:
            {
                var strValues = xe.Value.Split(",");
                property.Data = Array.ConvertAll(strValues, MathLibrary.HexToByte);
                
                return Result.OkExn(true);
            }
            case EIcVariantV01.Deprecated:
            {
                return Result.Err<bool>(new ArgumentOutOfRangeException());
            }
            case EIcVariantV01.ObjectId:
            {
                if (!ulong.TryParse(xe.Value, out var result))
                {
                    return Result.Err<bool>(new XmlSchemaException($"{xe.Value} is not a valid {property.Variant.ToXmlString()}"));
                }
                
                // todo: test this, might break
                property.Data = result;
                
                return Result.OkExn(true);
            }
            case EIcVariantV01.Events:
            {
                string[] eventStringArray = [xe.Value];
                if (xe.Value.Contains(','))
                {
                    eventStringArray = xe.Value.Split(", ");
                }
        
                property.Data = (from eventString in eventStringArray 
                        select eventString.Split("=") into eventStrings 
                        select Array.ConvertAll(eventStrings, MathLibrary.HexToUInt) into eventsArray 
                        select (eventsArray[0], eventsArray[1]))
                    .ToArray();
            }
                break;
            case EIcVariantV01.Total:
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