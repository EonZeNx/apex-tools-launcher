using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enum;
using ATL.Core.Extensions;
using ATL.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04;

public class AdfV04File
{
    public AdfV04Header Header = new();
    public List<AdfV04Type> Types = [];
    public List<AdfV04Type> InternalTypes = [];
    public List<string> Strings = [];
    public Dictionary<uint, string> StringHashes = [];

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

        if (!Strings.Contains(name))
        {
            Strings.Add(name);
        }

        var nameIndex = Strings.FindIndex(s => string.Equals(s, name));
        var definition = new AdfV04Type
        {
            Type = type,
            Size = size,
            Alignment = alignment,
            TypeHash = typeHash,
            NameIndex = (ulong) nameIndex,
            Flags = flags,
            ScalarType = scalarType,
            SubTypeHash = 0,
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

    public void AddTypes(Stream stream)
    {
        if (Header.FirstStringHashOffset > 0)
        { // TODO: This is untested
            stream.Seek(Header.FirstStringHashOffset, SeekOrigin.Begin);

            var strings = new string[Header.StringHashCount];
            for (var i = 0; i < Header.StringHashCount; i++)
            {
                strings[i] = stream.ReadStringZ();
            }
            
            for (var i = 0; i < Header.StringHashCount; i++)
            {
                var hash = stream.Read<ulong>();
                
                // Note: hashes are stored as uint64/ulong, but only 32 bits are used
                StringHashes.Add((uint) hash, strings[i]);
            }
        }

        if (Header.FirstStringDataOffset > 0)
        {
            stream.Seek(Header.FirstStringDataOffset, SeekOrigin.Begin);
            
            var strings = new string[Header.StringCount];
            var stringLengths = new byte[Header.StringCount];
        
            for (var i = 0; i < Header.StringCount; i += 1)
            {
                stringLengths[i] = (byte) (stream.Read<byte>() + 1);
            }
        
            for (var i = 0; i < Header.StringCount; i += 1)
            {
                strings[i] = stream.ReadStringOfLength(stringLengths[i]);
            }
            
            Strings.AddRange(strings);
        }

        if (Header.FirstTypeOffset > 0)
        {
            stream.Seek(Header.FirstTypeOffset, SeekOrigin.Begin);

            var types = new AdfV04Type[Header.TypeCount];
            for (var i = 0; i < Header.TypeCount; i += 1)
            {
                var optionType = stream.ReadAdfV04Type();
                if (!optionType.IsSome(out var adfType))
                    continue;

                if (HasType(adfType.TypeHash))
                {
                    stream.Seek(adfType.DataSize(), SeekOrigin.Current);
                    continue;
                }
                    
                types[i] = adfType;
                // TODO: Reindex string and member strings?
                // TODO: AdfV04Member Members here
                
                Types.Add(adfType);
                InternalTypes.Add(adfType);
            }
        }
    }

    public Stream ReadInstanceData(Stream stream, uint nameHash, uint typeHash)
    {
        var optionResult = Option<AdfV04Instance>.None;

        stream.Seek(Header.FirstInstanceOffset, SeekOrigin.Begin);
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

    public Option<AdfV04InstanceInfo> GetInstance(Stream stream)
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
            // TODO: String lookup
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
            xeStruct.SetAttributeValue("type", adfType.Type);

            for (var i = 0; i < adfType.MemberCountOrDataAlign; i += 1)
            {
                var member = adfType.Members[i];
                
                var optionSubType = FindType(member.TypeHash);
                if (!optionSubType.IsSome(out var subtype))
                    continue;
                
                var xeMember = new XElement("member");
                xeMember.SetAttributeValue("name",
                    Strings.Count > (int) member.NameIndex
                        ? Strings[(int) member.NameIndex]
                        : "name index out of bounds");
                WriteInstance(inBuffer, subtype, xeMember, offset + member.Offset);
            }
            
            break;
        }
        case EAdfV04Type.Pointer:
        case EAdfV04Type.Deferred:
            break;
        case EAdfV04Type.Array:
        {
            var relativeOffset = inBuffer.Read<uint>();
            var count = inBuffer.Read<uint>();
            
            var optionSubType = FindType(adfType.SubTypeHash);
            if (!optionSubType.IsSome(out var subtype))
                break;

            var xeArray = new XElement("array");
            xeArray.SetAttributeValue("type", subtype.Type);
            
            for (var i = 0; i < count; i += 1)
            {
                WriteInstance(inBuffer, subtype, xeArray, (uint) (relativeOffset + (subtype.Size * i)));
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
            var optionSubType = FindType(adfType.SubTypeHash);
            if (!optionSubType.IsSome(out var subtype))
                break;

            var subtypeSize = subtype.Size;
            if (subtype.Type is EAdfV04Type.String or EAdfV04Type.Pointer)
                subtypeSize = 8;
            
            var xeArray = new XElement("inline");
            xeArray.SetAttributeValue("type", subtype.Type);
            
            for (var i = 0; i < adfType.MemberCountOrDataAlign; i += 1)
            {
                WriteInstance(inBuffer, subtype, xeArray, (uint) (offset + (subtypeSize * i)));
                if (subtype.Type == EAdfV04Type.Scalar && i != 0)
                {
                    xeArray.Add(" ");
                }
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
    
    public XElement WriteInstances(Stream inBuffer)
    {
        inBuffer.Seek(Header.FirstInstanceOffset, SeekOrigin.Begin);

        var xeInstances = new XElement("instances");
        
        for (var i = 0; i < Header.InstanceCount; i++)
        {
            var optionInstanceInfo = GetInstance(inBuffer);
            if (!optionInstanceInfo.IsSome(out var instanceInfo))
                continue;

            if (instanceInfo.InstanceOffset == 0 || instanceInfo.InstanceSize == 0)
            {
                continue;
            }

            var xeInstance = new XElement("instance");
            
            var optionAdfType = FindType(instanceInfo.TypeHash);
            if (!optionAdfType.IsSome(out var adfType))
                continue;

            WriteInstance(inBuffer, adfType, xeInstance, instanceInfo.InstanceOffset);
            
            xeInstances.Add(xeInstance);
        }

        return xeInstances;
    }
}
