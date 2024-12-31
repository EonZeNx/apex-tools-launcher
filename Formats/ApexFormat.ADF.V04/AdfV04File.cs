using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Config;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04;

public class AdfV04File
{
    public AdfV04Header Header = new();
    public List<AdfV04Type> Types = [];
    public List<AdfV04Type> InternalTypes = [];
    // Note: can contain null characters
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
            adfType.Name = GetString(adfType.NameIndex, localStringTable, false);
            adfType.NameIndex = (uint) GetStringIndex(adfType.Name);

            switch (adfType.Type)
            {
                case EAdfV04Type.Struct:
                {
                    for (var j = 0; j < adfType.MemberCountOrDataAlign; j++)
                    {
                        var member = adfType.Members[j];
                        var memberName = localStringTable[(int) member.NameIndex];
                        adfType.Members[j].Name = memberName;
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
                        adfType.EnumFlags[j].Name = enumName;
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

    public Option<AdfV04InstanceInfo> GetInstanceInfo(Stream stream, string[] localStringTable)
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
        
        var infoName = localStringTable[(int) result.NameIndex];
        result.Name = infoName;
        result.NameIndex = (uint) GetStringIndex(infoName);

        var optionAdfType = FindType(result.TypeHash);
        if (optionAdfType.IsNone)
            return Option<AdfV04InstanceInfo>.None;

        result.InstanceOffset = instance.PayloadOffset;
        result.InstanceSize = instance.PayloadSize;
        
        return Option.Some(result);
    }

    public void WriteInstance(Stream inBuffer, AdfV04Type adfType, XElement parent, uint instanceOffset, uint offset = 0)
    {
        inBuffer.Seek(instanceOffset + offset, SeekOrigin.Begin);
        
        switch (adfType.Type)
        {
        case EAdfV04Type.Scalar:
            parent.SetAttributeValue("type", GetString(adfType.NameIndex));
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
                
                WriteInstance(inBuffer, memberType, xeMember, instanceOffset, memberDataOffset);
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
            
            var flags = inBuffer.Read<uint>();
            var count = inBuffer.Read<uint>();
            
            for (var i = 0; i < count; i += 1)
            {
                var childOffset = instanceOffset + arrayOffset + (uint) (subtype.Size * i);
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
                
                WriteInstance(inBuffer, subtype, xeArray, (uint) (instanceOffset + offset + (subtypeSize * i)));
            }
            
            parent.Add(xeArray);
            
            break;
        }
        case EAdfV04Type.String:
        {
            var xeString = new XElement("string");
            
            var stringOffset = inBuffer.Read<uint>();
            if (stringOffset <= 0)
            {
                parent.Add(xeString);
                break;
            }

            inBuffer.Seek(instanceOffset + stringOffset, SeekOrigin.Begin);
            var value = inBuffer.ReadStringZ();
            xeString.Add(value);
            
            parent.Add(xeString);
            break;
        }
        case EAdfV04Type.Recursive:
            break;
        case EAdfV04Type.Bitfield:
            break;
        case EAdfV04Type.Enum:
        {
            var xElement = new XElement("enum");
            
            var enumValue = inBuffer.Read<uint>();
            xElement.Add(enumValue);
            
            parent.Add(xElement);
            break;
        }
        case EAdfV04Type.StringHash:
        {
            var xElement = new XElement("stringhash");
            
            var stringHash = inBuffer.Read<uint>();
            var value = $"{stringHash.ReverseEndian():X8}";
            if (CoreConfig.AppConfig.Cli.LookupHash)
            {
                var optionResult = HashDatabases.Lookup(stringHash);
                if (optionResult.IsSome(out var lookupResult))
                {
                    value = lookupResult.Value;
                }
            }
            xElement.Add(value);
            
            parent.Add(xElement);
            break;
        }
        default:
            throw new ArgumentOutOfRangeException();
        }
    }

    public Option<XElement> WriteScalar(Stream stream, AdfV04Type adfType, string name)
    {
        var xe = new XElement("member");
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", GetString(adfType.NameIndex));
        
        switch (adfType.ScalarType)
        {
        case EAdfV04ScalarType.Signed:
            switch (adfType.Size)
            {
                case sizeof(sbyte): xe.Add(stream.Read<sbyte>()); break;
                case sizeof(short): xe.Add(stream.Read<short>()); break;
                case sizeof(int): xe.Add(stream.Read<int>()); break;
                case sizeof(long): xe.Add(stream.Read<long>()); break;
            }
            break;
        case EAdfV04ScalarType.Unsigned:
            switch (adfType.Size)
            {
                case sizeof(byte): xe.Add(stream.Read<byte>()); break;
                case sizeof(ushort): xe.Add(stream.Read<ushort>()); break;
                case sizeof(uint): xe.Add(stream.Read<uint>()); break;
                case sizeof(ulong): xe.Add(stream.Read<ulong>()); break;
            }
            break;
        case EAdfV04ScalarType.Float:
            switch (adfType.Size)
            {
                case sizeof(float): xe.Add(stream.Read<float>()); break;
                case sizeof(double): xe.Add(stream.Read<double>()); break;
            }
            break;
        default:
            throw new ArgumentOutOfRangeException();
        }

        return Option.Some(xe);
    }

    public Option<XElement> WriteStruct(Stream stream, AdfV04InstanceInfo instanceInfo, AdfV04Type adfType, string name)
    {
        var xe = new XElement(adfType.Type.ToXString());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.Name.RemoveAll(CharactersToRemove));
        xe.SetAttributeValue("type-hash", adfType.TypeHash);
        
        foreach (var member in adfType.Members)
        {
            var optionMemberType = FindType(member.TypeHash);
            if (!optionMemberType.IsSome(out var memberType))
                continue;

            var memberName = member.Name.RemoveAll(CharactersToRemove);
            
            var optionXMember = WriteXInstanceData(stream, instanceInfo, memberType, memberName);
            if (!optionXMember.IsSome(out var xMember))
                continue;
            
            xe.Add(xMember);
        }

        return Option.Some(xe);
    }
    
    public Option<XElement> WriteArray(Stream stream, AdfV04InstanceInfo instanceInfo, AdfV04Type adfType, string name)
    {
        var xe = new XElement(adfType.Type.ToXString());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.Name.RemoveAll(CharactersToRemove));
        xe.SetAttributeValue("type-hash", adfType.TypeHash);
        
        var arrayOffset = stream.Read<uint>();
        if (arrayOffset <= 0)
            return Option.Some(xe);
        
        var optionSubType = FindType(adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Some(xe);
        
        var flags = stream.Read<uint>();
        var count = stream.Read<uint>();

        for (var i = 0; i < count; i += 1)
        {
            var dataOffset = instanceInfo.InstanceOffset + arrayOffset + subtype.Size * i;
            stream.Seek(dataOffset, SeekOrigin.Begin);

            var optionXChild = Option<XElement>.None;
            switch (subtype.Type)
            {
                case EAdfV04Type.Scalar:
                    optionXChild = WriteScalar(stream, subtype, name);
                    break;
                case EAdfV04Type.Struct:
                    optionXChild = WriteStruct(stream, instanceInfo, subtype, string.Empty);
                    break;
            }

            if (!optionXChild.IsSome(out var xChild))
                continue;
            
            xe.Add(xChild);
            if (subtype.Type == EAdfV04Type.Scalar && i != 0)
            {
                xe.Add(" ");
            }
        }

        return Option.Some(xe);
    }

    public Option<XElement> WriteXInstanceData(Stream stream, AdfV04InstanceInfo instanceInfo, AdfV04Type adfType, string name)
    {
        var optionXMember = Option.None<XElement>();
        switch (adfType.Type)
        {
            case EAdfV04Type.Scalar:
                optionXMember = WriteScalar(stream, adfType, name);
                break;
            case EAdfV04Type.Struct:
                optionXMember = WriteStruct(stream, instanceInfo, adfType, name);
                break;
            case EAdfV04Type.Pointer:
                break;
            case EAdfV04Type.Array:
                optionXMember = WriteArray(stream, instanceInfo, adfType, name);
                break;
            case EAdfV04Type.InlineArray:
                break;
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
            case EAdfV04Type.Deferred:
                break;
            default:
                return Option.None<XElement>();
        }

        if (!optionXMember.IsSome(out var xMember))
            return Option.None<XElement>();
        
        return Option.Some(xMember);
    }
    
    public Option<XElement> WriteXInstance(Stream stream, AdfV04InstanceInfo instanceInfo)
    {
        var xe = new XElement("instance");
        
        var optionAdfType = FindType(instanceInfo.TypeHash);
        if (!optionAdfType.IsSome(out var adfType))
            return Option.None<XElement>();
        
        xe.SetAttributeValue("name", instanceInfo.Name.RemoveAll(CharactersToRemove));
        xe.SetAttributeValue("type", adfType.Name.RemoveAll(CharactersToRemove));
        xe.SetAttributeValue("type-hash", adfType.TypeHash);

        foreach (var member in adfType.Members)
        {
            var optionMemberType = FindType(member.TypeHash);
            if (!optionMemberType.IsSome(out var memberType))
                continue;

            var name = member.Name.RemoveAll(CharactersToRemove);
            
            var optionXMember = WriteXInstanceData(stream, instanceInfo, memberType, name);
            if (!optionXMember.IsSome(out var xMember))
                continue;
            
            xe.Add(xMember);
        }

        return Option.Some(xe);
    }
    
    public XElement WriteXInstances(Stream inBuffer, string[] localStringTable)
    {
        inBuffer.Seek(Header.InstanceOffset, SeekOrigin.Begin);

        var xeInstances = new XElement("instances");
        
        for (var i = 0; i < Header.InstanceCount; i++)
        {
            var optionInstanceInfo = GetInstanceInfo(inBuffer, localStringTable);
            if (!optionInstanceInfo.IsSome(out var instanceInfo))
                continue;

            if (instanceInfo.InstanceOffset == 0 || instanceInfo.InstanceSize == 0)
                continue;

            var optionXInstance = WriteXInstance(inBuffer, instanceInfo);
            if (!optionXInstance.IsSome(out var xInstance))
                continue;
            
            xeInstances.Add(xInstance);
        }

        return xeInstances;
    }
}
