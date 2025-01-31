using System.Xml;
using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using CommunityToolkit.HighPerformance;
using RustyOptions;
using AdfV04TypeLibrary = ApexFormat.ADF.V04.Class.AdfV04TypeLibrary;

namespace ApexFormat.ADF.V04;

public class AdfV04File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "adf";

    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !fileStream.ReadAdfV04Header().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
    }

    public Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        var optionHeader = inStream.ReadAdfV04Header();
        if (!optionHeader.IsSome(out var header))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract {nameof(AdfV04Header)}"));

        var resultStringHashes = ExtractStringHashes(inStream, header);
        if (resultStringHashes.IsErr(out var shEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to extract string hashes: {shEx}"));
        }

        var stringHashes = resultStringHashes.Unwrap();

        var resultStringTable = ExtractStringTable(inStream, header);
        if (resultStringTable.IsErr(out var stEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to extract string table: {stEx}"));
        }

        var stringTable = resultStringTable.Unwrap();

        var resultTypes = ExtractTypes(inStream, header);
        if (resultTypes.IsErr(out var tEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to extract types: {tEx}"));
        }

        var types = resultTypes.Unwrap();
        foreach (var type in types)
        {
            type.FindOrInsertName(ref stringTable);
        }

        var optionXInstances = ToXElement(inStream, header, stringHashes, stringTable, types);
        if (!optionXInstances.IsSome(out var xInstances))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract instances"));
        
        var xStringHashes = XElementBuilder.Create("string_hashes")
            .WithChildren(stringHashes, kvp => XElementBuilder.Create("string_hash")
                .WithAttribute("name", kvp.Value)
                .WithAttribute("hash", $"{kvp.Key:X08}")
                .Build())
            .Build();
        
        var xStringTable = XElementBuilder.Create("string_table")
            .WithChildren(stringTable, s => XElementBuilder.Create("string")
                .WithContent(s.Trim().TrimEnd((char) 0x00))
                .Build())
            .Build();
        
        var xTypes = XElementBuilder.Create("types")
            .WithChildren(types, t => t.ToXElement())
            .Build();
        
        var xd = XProjectBuilder.CreateXProjectBuilder()
            .WithType(AdfV04FileLibrary.XName)
            .WithVersion(AdfV04FileLibrary.Version.ToString())
            .WithExtension(ExtractExtension)
            .WithChild(xInstances)
            // .WithChild(xStringHashes)
            // .WithChild(xStringTable)
            .WithChild(xTypes)
            .Build();

        using var xw = XmlWriter.Create(outStream, XDocumentLibrary.XmlWriterSettings);
        xd.Save(xw);

        return Result.OkExn(0);
    }

    public Result<int, Exception> ExtractPathToPath(string inPath, string outPath)
    {
        using var inStream = new FileStream(inPath, FileMode.Open);
        
        ExtractExtension = Path.GetExtension(inPath).Trim('.');
        var fileName = Path.GetFileNameWithoutExtension(inPath);
        var xmlFilePath = Path.Join(outPath, $"{fileName}.xml");
        
        using var outStream = new FileStream(xmlFilePath, FileMode.Create);
        var result = ExtractStreamToStream(inStream, outStream);
        
        return result;
    }

    public bool CanRepackPath(string path)
    {
        return false;
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        return Result.Err<int>(new NotImplementedException());
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        return Result.Err<int>(new NotImplementedException());
    }

    public Result<Dictionary<uint, string>, Exception> ExtractStringHashes(Stream stream, AdfV04Header header)
    {
        var stringHashes = new Dictionary<uint, string>();
        if (header.StringHashOffset <= 0 || header.StringHashOffset >= stream.Length || header.StringHashCount == 0)
        {
            return Result.OkExn(stringHashes);
        }
        
        stream.Seek(header.StringHashOffset, SeekOrigin.Begin);

        for (var i = 0; i < header.StringHashCount; i++)
        {
            var value = stream.ReadStringZ();
            var hash = stream.Read<ulong>();
                
            // note: hashes are stored as uint64/ulong, but only 32 bits are used
            stringHashes.TryAdd((uint) hash, value);
        }

        return Result.OkExn(stringHashes);
    }

    public Result<string[], Exception> ExtractStringTable(Stream stream, AdfV04Header header)
    {
        var stringTable = new List<string>();
        if (header.StringTableOffset <= 0 || header.StringTableOffset >= stream.Length || header.StringTableCount == 0)
        {
            return Result.OkExn(stringTable.ToArray());
        }
        
        if (header.StringTableOffset + header.StringTableCount >= stream.Length)
        {
            return Result.Err<string[]>(new InvalidOperationException($"Failed to extract string table, count exceeds stream length"));
        }
        
        stream.Seek(header.StringTableOffset, SeekOrigin.Begin);
        
        var stringLengths = new byte[header.StringTableCount];
        for (var i = 0; i < header.StringTableCount; i += 1)
        {
            stringLengths[i] = stream.Read<byte>();
        }

        foreach (var stringLength in stringLengths)
        {
            if (!stream.CouldRead(stringLength + 1))
            {
                return Result.Err<string[]>(new InvalidOperationException($"Failed to extract string table, string exceeds stream length"));
            }
            
            // note: null terminated string
            var value = stream.ReadStringOfLength(stringLength + 1);
            stringTable.Add(value);
        }

        return Result.OkExn(stringTable.ToArray());
    }

    public static AdfV04Type[] CreateInbuiltTypes()
    {
        var result = new[]
        {
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Unsigned, sizeof(byte), "uint8"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(sbyte), "int8"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Unsigned, sizeof(ushort), "uint16"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(short), "int16"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Unsigned, sizeof(uint), "uint32"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(int), "int32"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Unsigned, sizeof(ulong), "uint64"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Signed, sizeof(long), "int64"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Float, sizeof(float), "float"),
            CreateInbuiltType(EAdfV04Type.Scalar, EAdfV04ScalarType.Float, sizeof(double), "double"),
            CreateInbuiltType(EAdfV04Type.String, EAdfV04ScalarType.Signed, 8, "String", 0),
            CreateInbuiltType(EAdfV04Type.Deferred, EAdfV04ScalarType.Signed, 16, "void", 0),
        };
        
        return result;
    }
    
    public static AdfV04Type CreateInbuiltType(EAdfV04Type type, EAdfV04ScalarType scalarType, uint size, string name, ushort flags = 3)
    {
        var typeName = $"{name}{(uint) type}{size}{size}";
        var typeHash = typeName.HashJenkins();
        var alignment = size;

        if (type == EAdfV04Type.Deferred)
        {
            typeHash = 0xDEFE88ED;
            alignment = 8;
        }

        var definition = new AdfV04Type
        {
            Type = type,
            Size = size,
            Alignment = alignment,
            TypeHash = typeHash,
            NameIndex = ulong.MaxValue,
            Name = name,
            Flags = flags,
            ScalarType = scalarType,
            ScalarTypeHash = 0,
            BitCountOrArrayLength = 0,
            MemberCountOrDataAlign = 0,
        };

        return definition;
    }

    public Result<AdfV04Type[], Exception> ExtractTypes(Stream stream, AdfV04Header header)
    {
        var types = new List<AdfV04Type>();
        if (header.TypeOffset <= 0 || header.TypeOffset >= stream.Length || header.TypeCount == 0)
        {
            return Result.OkExn(types.ToArray());
        }
        
        if (header.TypeOffset + header.TypeCount * AdfV04TypeLibrary.SizeOf >= stream.Length)
        {
            return Result.Err<AdfV04Type[]>(new InvalidOperationException($"Failed to extract adf types, count exceeds stream length"));
        }
        
        stream.Seek(header.TypeOffset, SeekOrigin.Begin);
        for (var i = 0; i < header.TypeCount; i += 1)
        {
            var optionType = stream.ReadAdfV04Type();
            if (!optionType.IsSome(out var adfType))
            {
                return Result.Err<AdfV04Type[]>(new InvalidOperationException($"Failed to extract adf types, exceeded stream length"));
            }
            
            types.Add(adfType);
        }
        
        types.AddRange(CreateInbuiltTypes());
        
        return Result.OkExn(types.ToArray());
    }

    public Option<XElement> ToXElement(Stream stream, AdfV04Header header, Dictionary<uint, string> stringHashes, string[] stringTable, AdfV04Type[] types)
    {
        var xe = new XElement("instances");
        
        stream.Seek(header.InstanceOffset, SeekOrigin.Begin);
        for (var i = 0; i < header.InstanceCount; i += 1)
        {
            stream.Seek(header.InstanceOffset + AdfV04InstanceLibrary.SizeOf * i, SeekOrigin.Begin);
            
            var optionInstance = stream.ReadAdfV04Instance();
            if (!optionInstance.IsSome(out var instance))
                return Option.Create(xe);

            if (instance.PayloadOffset == 0 || instance.PayloadSize == 0)
                return Option.Create(xe);
            
            stream.Seek(instance.PayloadOffset, SeekOrigin.Begin);

            instance.TryFindName(stringTable);
            
            var optionXInstance = instance.ToXElement(stream, types);
            if (!optionXInstance.IsSome(out var xInstance))
                return Option.Create(xe);
            
            xe.Add(xInstance);
        }
        
        return Option.Create(xe);
    }
}

public static class AdfV04FileLibrary
{
    public const string XName = "adf";
    public const int Version = 4;

    public static readonly string VersionName = $"{XName.ToUpper()} v{Version:D2}";
}
