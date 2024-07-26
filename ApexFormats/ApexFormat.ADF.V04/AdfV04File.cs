using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enum;
using ATL.Core.Extensions;
using ATL.Core.Hash;
using ATL.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04;

public class AdfV04File
{
    public AdfV04Header Header = new();
    public List<AdfV04Type> Types = [];
    public List<AdfV04Type> InternalTypes = [];
    // Note: can container null characters
    public List<string> StringTable = [];
    public Dictionary<uint, string> StringHashes = [];
    
    private static readonly string[] CharactersToRemove = ["\0"];

    public void AddBuiltInType(EAdfV04Type type, EAdfV04ScalarType scalarType, uint size, string name, ushort flags = 3)
    {
        var typeName = $"{name}{(uint) type}{size}{size}";
        var typeHash = typeName.HashJenkins();
        var alignment = size;

        if (type == EAdfV04Type.Deferred)
        {
            typeHash = 0xDEFE88ED;
            alignment = 8;
        }

        if (!StringTable.Contains(name))
        {
            StringTable.Add(name);
        }

        var nameIndex = StringTable.FindIndex(s => string.Equals(s, name));
        var definition = new AdfV04Type
        {
            Type = type,
            Size = size,
            Alignment = alignment,
            TypeHash = typeHash,
            NameIndex = (ulong) nameIndex,
            Flags = flags,
            ScalarType = scalarType,
            ScalarTypeHash = 0,
            BitCountOrArrayLength = 0,
            MemberCountOrDataAlign = 0,
        };

        Types.Add(definition);
    }
    
    public void AddBuiltInTypes()
    {
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(byte), "uint8");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(sbyte), "int8");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(ushort), "uint16");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(short), "int16");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(uint), "uint32");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(int), "int32");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(ulong), "uint64");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(long), "int64");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Float, sizeof(float), "float");
        AddBuiltInType(EAdfV04Type.Scalar, EAdfV04ScalarType.Float, sizeof(double), "double");
        AddBuiltInType(EAdfV04Type.String, EAdfV04ScalarType.Signed, 8, "String", 0);
        AddBuiltInType(EAdfV04Type.Deferred, EAdfV04ScalarType.Signed, 16, "void", 0);
    }

    public bool HasType(uint typeHash)
    {
        return Types.Any(t => t.TypeHash == typeHash);
    }
    
    public Option<AdfV04Type> FindType(uint typeHash)
    {
        return Types.FirstOrNone(t => t.TypeHash == typeHash);
    }

    public static string GetString(ulong index, string[] stringTable, bool removeCharacters = true)
    {
        var result = stringTable[(int) index];
        if (removeCharacters)
            result = result.RemoveAll(CharactersToRemove);

        return result;
    }
    
    public string GetString(ulong index, bool removeCharacters = true)
    {
        return GetString(index, StringTable.ToArray(), removeCharacters);
    }
    
    public int GetStringIndex(string value)
    {
        return StringTable.FindIndex(s => string.Equals(s, value));
    }

    public int ReadStringHashes(Stream stream)
    {
        var count = 0;

        if (Header.StringHashOffset <= 0)
            return count;
        
        stream.Seek(Header.StringHashOffset, SeekOrigin.Begin);

        for (var i = 0; i < Header.StringHashCount; i++)
        {
            var value = stream.ReadStringZ();
            var hash = stream.Read<ulong>();
                
            // Note: hashes are stored as uint64/ulong, but only 32 bits are used
            count += StringHashes.TryAdd((uint) hash, value) ? 1 : 0;
        }

        return count;
    }
    
    public string[] ReadStringTable(Stream stream)
    {
        var results = new List<string>();
        
        if (Header.StringTableOffset <= 0)
            return results.ToArray();
        
        stream.Seek(Header.StringTableOffset, SeekOrigin.Begin);
            
        var stringLengths = new byte[Header.StringTableCount];
        for (var i = 0; i < Header.StringTableCount; i += 1)
        {
            stringLengths[i] = (byte) (stream.Read<byte>() + 1);
        }
        
        for (var i = 0; i < Header.StringTableCount; i += 1)
        {
            var value = stream.ReadStringOfLength(stringLengths[i]);
            results.Add(value);
        }

        StringTable.AddRange(results);
        return results.ToArray();
    }
    
    public int ReadTypes(Stream stream, string[] localStringTable)
    {
        if (Header.TypeOffset <= 0)
            return 0;
        
        stream.Seek(Header.TypeOffset, SeekOrigin.Begin);

        var count = 0;
        var types = new AdfV04Type[Header.TypeCount];
        for (var i = 0; i < Header.TypeCount; i += 1)
        {
            var optionType = stream.ReadAdfV04Type();
            if (!optionType.IsSome(out var adfType))
                continue;

            if (HasType(adfType.TypeHash))
                continue;

            // reindex name, loading several types at once can displace it
            var name = GetString(adfType.NameIndex, localStringTable, false);
            adfType.NameIndex = (uint) GetStringIndex(name);

            switch (adfType.Type)
            {
                case EAdfV04Type.Struct:
                {
                    for (var j = 0; j < adfType.MemberCountOrDataAlign; j++)
                    {
                        var member = adfType.Members[j];
                        var memberName = localStringTable[(int) member.NameIndex];
                        adfType.Members[j].NameIndex = (uint) GetStringIndex(memberName);
                    }
                    break;
                }
                case EAdfV04Type.Enum:
                {
                    for (var j = 0; j < adfType.MemberCountOrDataAlign; j++)
                    {
                        var enumFlag = adfType.EnumFlags[j];
                        var enumName = localStringTable[(int) enumFlag.NameIndex];
                        adfType.EnumFlags[j].NameIndex = (uint) GetStringIndex(enumName);
                    }
                    break;
                }
            }
                
            types[i] = adfType;
            
            Types.Add(adfType);
            InternalTypes.Add(adfType);

            count += 1;
        }

        return count;
    }

    public Stream ReadInstanceData(Stream stream, uint nameHash, uint typeHash)
    {
        var optionResult = Option<AdfV04Instance>.None;

        stream.Seek(Header.InstanceOffset, SeekOrigin.Begin);
        for (var i = 0; i < Header.InstanceCount; i++)
        {
            var optionInstance = stream.ReadAdfV04Instance();
            if (!optionInstance.IsSome(out var instance))
                continue;

            if (instance.NameHash != nameHash || instance.TypeHash != typeHash)
                continue;
            
            optionResult = optionInstance;
            break;
        }

        if (!optionResult.IsSome(out var result))
            return new MemoryStream();

        var optionAdfType = FindType(result.TypeHash);
        if (!optionAdfType.IsSome(out var adfType))
        {
            return new MemoryStream();
        }

        stream.Seek(result.PayloadOffset, SeekOrigin.Begin);

        var instanceData = new MemoryStream(new byte[result.PayloadSize]);
        stream.CopyToLimit(instanceData, (int) result.PayloadSize);
        
        var size = stream.Read<uint>();
        instanceData.Seek(0, SeekOrigin.Begin);

        var has32BitInlineArrays = Header.Flags.HasFlag(EAdfV04HeaderFlags.RelativeOffsetExists);
        if (has32BitInlineArrays)
        { // TODO: Load inline offsets
            ;
        }
        else
        { // instance relative offsets to file absolute offsets
            ulong currentOffset = 0;
            ulong v72 = 0;

            while (size > 0)
            {
                currentOffset += size;
                
                instanceData.Seek((long) currentOffset - 4, SeekOrigin.Begin);
                v72 = instanceData.Read<uint>();
                if (v72 == 1)
                    v72 = 0;
                
                size = instanceData.Read<uint>();
                
                instanceData.Seek((long) currentOffset - 4, SeekOrigin.Begin);
                instanceData.Write(result.PayloadOffset + v72);
            }
        }
        
        return instanceData;
    }

    public Option<AdfV04InstanceInfo> GetInstanceInfo(Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Instance.SizeOf())
        {
            return Option<AdfV04InstanceInfo>.None;
        }

        var optionInstance = stream.ReadAdfV04Instance();
        if (!optionInstance.IsSome(out var instance))
            return Option<AdfV04InstanceInfo>.None;

        var result = new AdfV04InstanceInfo
        {
            NameHash = instance.NameHash,
            TypeHash = instance.TypeHash,
            NameIndex = instance.NameIndex
        };

        var optionAdfType = FindType(result.TypeHash);
        if (optionAdfType.IsNone)
            return Option<AdfV04InstanceInfo>.None;

        result.InstanceOffset = instance.PayloadOffset;
        result.InstanceSize = instance.PayloadSize;
        
        return Option.Some(result);
    }

    public void WriteInstance(Stream inBuffer, AdfV04Type adfType, XElement parent, uint offset = 0)
    {
        inBuffer.Seek(offset, SeekOrigin.Begin);
        
        switch (adfType.Type)
        {
        case EAdfV04Type.Scalar:
            switch (adfType.ScalarType)
            {
            case EAdfV04ScalarType.Signed:
                switch (adfType.Size)
                {
                    case sizeof(sbyte): parent.Add(inBuffer.Read<sbyte>()); break;
                    case sizeof(short): parent.Add(inBuffer.Read<short>()); break;
                    case sizeof(int): parent.Add(inBuffer.Read<int>()); break;
                    case sizeof(long): parent.Add(inBuffer.Read<long>()); break;
                }
                break;
            case EAdfV04ScalarType.Unsigned:
                switch (adfType.Size)
                {
                    case sizeof(byte): parent.Add(inBuffer.Read<byte>()); break;
                    case sizeof(ushort): parent.Add(inBuffer.Read<ushort>()); break;
                    case sizeof(uint): parent.Add(inBuffer.Read<uint>()); break;
                    case sizeof(ulong): parent.Add(inBuffer.Read<ulong>()); break;
                }
                break;
            case EAdfV04ScalarType.Float:
                switch (adfType.Size)
                {
                    case sizeof(float): parent.Add(inBuffer.Read<float>()); break;
                    case sizeof(double): parent.Add(inBuffer.Read<double>()); break;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            break;
        case EAdfV04Type.Struct:
        {
            var xeStruct = new XElement("struct");
            xeStruct.SetAttributeValue("type", GetString(adfType.NameIndex));

            for (var i = 0; i < adfType.MemberCountOrDataAlign; i += 1)
            {
                var member = adfType.Members[i];
                
                var optionMemberType = FindType(member.TypeHash);
                if (!optionMemberType.IsSome(out var memberType))
                    continue;
                
                var xeMember = new XElement("member");
                xeMember.SetAttributeValue("name", GetString(member.NameIndex));

                var memberDataOffset = offset + member.Offset;
                memberDataOffset = MathLibrary.Align(memberDataOffset, memberType.Alignment);
                
                WriteInstance(inBuffer, memberType, xeMember, memberDataOffset);
                xeStruct.Add(xeMember);
            }
            
            parent.Add(xeStruct);
            break;
        }
        case EAdfV04Type.Pointer:
        case EAdfV04Type.Deferred:
            break;
        case EAdfV04Type.Array:
        {
            var xeArray = new XElement("array");
            
            var arrayOffset = inBuffer.Read<uint>();
            if (arrayOffset <= 0)
            {
                parent.Add(xeArray);
                break;
            }
            
            var optionSubType = FindType(adfType.ScalarTypeHash);
            if (!optionSubType.IsSome(out var subtype))
                break;
            
            xeArray.SetAttributeValue("type", GetString(subtype.NameIndex));
            
            var unknown01 = inBuffer.Read<uint>();
            var count = inBuffer.Read<uint>();
            
            for (var i = 0; i < count; i += 1)
            {
                var childOffset = arrayOffset + (uint) (subtype.Size * i);
                WriteInstance(inBuffer, subtype, xeArray, childOffset);
                if (subtype.Type == EAdfV04Type.Scalar && i != 0)
                {
                    xeArray.Add(" ");
                }
            }
            
            parent.Add(xeArray);
            break;
        }
        case EAdfV04Type.InlineArray:
        {
            var optionSubType = FindType(adfType.ScalarTypeHash);
            if (!optionSubType.IsSome(out var subtype))
                break;

            var subtypeSize = subtype.Size;
            if (subtype.Type is EAdfV04Type.String or EAdfV04Type.Pointer)
                subtypeSize = 8;
            
            var xeArray = new XElement("inline");
            xeArray.SetAttributeValue("type", GetString(subtype.NameIndex));
            
            for (var i = 0; i < adfType.BitCountOrArrayLength; i += 1)
            {
                if (subtype.Type == EAdfV04Type.Scalar && i != 0)
                {
                    xeArray.Add(" ");
                }
                
                WriteInstance(inBuffer, subtype, xeArray, (uint) (offset + (subtypeSize * i)));
            }
            
            parent.Add(xeArray);
            
            break;
        }
        case EAdfV04Type.String:
            break;
        case EAdfV04Type.Recursive:
            break;
        case EAdfV04Type.Bitfield:
            break;
        case EAdfV04Type.Enum:
            break;
        case EAdfV04Type.StringHash:
            break;
        default:
            throw new ArgumentOutOfRangeException();
        }
    }
    
    public XElement WriteInstances(Stream inBuffer, string[] localStringTable)
    {
        inBuffer.Seek(Header.InstanceOffset, SeekOrigin.Begin);

        var xeInstances = new XElement("instances");
        
        for (var i = 0; i < Header.InstanceCount; i++)
        {
            var optionInstanceInfo = GetInstanceInfo(inBuffer);
            if (!optionInstanceInfo.IsSome(out var instanceInfo))
                continue;

            if (instanceInfo.InstanceOffset == 0 || instanceInfo.InstanceSize == 0)
                continue;
            
            var optionAdfType = FindType(instanceInfo.TypeHash);
            if (!optionAdfType.IsSome(out var adfType))
                continue;

            var xeInstance = new XElement("instance");
            var name = GetString(instanceInfo.NameIndex, localStringTable);
                
            xeInstance.SetAttributeValue("name", name.RemoveAll(CharactersToRemove));
            xeInstance.SetAttributeValue("type", $"{instanceInfo.TypeHash.ReverseEndian():X8}");
            WriteInstance(inBuffer, adfType, xeInstance, instanceInfo.InstanceOffset);
            
            xeInstances.Add(xeInstance);
        }

        return xeInstances;
    }
}
