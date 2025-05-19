using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ApexFormat.ADF.V04.Enums;
using ApexFormat.ADF.V04.Libraries;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using CommunityToolkit.HighPerformance;
using RustyOptions;

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
            return false;
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
        
        var xd = XProjectBuilder.Create()
            .WithType(AdfV04FileLibrary.XName)
            .WithVersion(AdfV04FileLibrary.Version.ToString())
            .WithExtension(ExtractExtension)
            .WithChild(xInstances)
            .WithChild(xStringHashes)
            .WithChild(xStringTable)
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
        if (!File.Exists(path))
            return false;

        try
        {
            var project = XProjectBuilder.Load(path);
            if (!project.Type.IsSome(out var projectType))
                return false;

            if (!string.Equals(projectType, AdfV04FileLibrary.XName))
                return false;
            
            if (!project.Version.IsSome(out var projectVersionString))
                return false;

            if (!int.TryParse(projectVersionString, out var projectVersion))
                return false;

            if (projectVersion != AdfV04FileLibrary.Version)
                return false;
            
            if (project.Extension.IsSome(out var projectExtension))
                ExtractExtension = projectExtension;
            
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        var xe = XElement.Load(inPath);
        
        var optionType = xe.GetAttribute("type");
        if (!optionType.IsSome(out var xeType))
            return Result.Err<int>(new InvalidOperationException("Missing type attribute"));
        
        if (!string.Equals(xeType, AdfV04FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element name is {xe.Name} not {AdfV04FileLibrary.XName}"));
        }

        if (xe.GetAttribute("extension").IsSome(out var extension))
        {
            ExtractExtension = extension;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(inPath);
        var repackFilePath = Path.Join(outPath, $"{fileName}.{ExtractExtension}");
        
        using var inStream = new FileStream(inPath, FileMode.Open);
        using var outStream = new FileStream(repackFilePath, FileMode.Create);
        
        return RepackStreamToStream(inStream, outStream);
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        var xe = XElement.Load(inStream);
        
        var optionType = xe.GetAttribute("type");
        if (!optionType.IsSome(out var xeType))
            return Result.Err<int>(new InvalidOperationException("Missing type attribute"));
        
        if (!string.Equals(xeType, AdfV04FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element name is {xe.Name} not {AdfV04FileLibrary.XName}"));
        }

        if (xe.GetAttribute("extension").IsSome(out var extension))
        {
            ExtractExtension = extension;
        }
        
        // load string hashes
        var resultStringHashes = RepackStringHashes(xe);
        if (resultStringHashes.IsErr(out var shEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack string hashes: {shEx}"));
        }

        var stringHashes = resultStringHashes.Unwrap();
        
        // load string table
        var resultStringTable = StringTableFromElement(xe);
        if (resultStringTable.IsErr(out var stEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack string table: {stEx}"));
        }

        var stringTable = resultStringTable.Unwrap();
        
        // load types
        var resultTypes = TypesFromElement(xe);
        if (resultTypes.IsErr(out var tEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack types: {tEx}"));
        }

        var types = resultTypes.Unwrap();
        
        // reindex type name indices
        var optionReindex = ReindexTypeNameIndices(ref types, stringTable);
        if (optionReindex.IsSome(out var rEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to reindex types: {rEx}"));
        }
        
        // skip header (or write dummy data)
        var header = new AdfV04Header
        {
            InstanceCount = 0,
            TypeCount = (uint) types.Length,
            StringHashCount = (uint) stringHashes.Count,
            StringTableCount = (uint) stringTable.Length,
            Unknown01 = 32,
            Unknown02 = 32
        };
        header.Write(outStream);
        outStream.AlignWrite(16, 0x00);
        
        // get instances
        var resultInstances = FromXElement(xe, stringTable, types);
        if (resultInstances.IsErr(out var iEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to get instances: {iEx}"));
        }
        
        var instances = resultInstances.Unwrap();
        
        // write instances
        var resultData = RepackInstances(xe, outStream, ref header, ref instances, stringHashes, stringTable, types);
        if (resultData.IsErr(out _))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack instances: {iEx}"));
        }
        
        // write types
        var optionRepackTypes = RepackTypes(outStream, ref header, types);
        if (optionRepackTypes.IsSome(out var typesEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack types: {typesEx}"));
        }
        
        // write string hashes
        var optionRepackStringHashes = RepackStringHashes(outStream, ref header, stringHashes);
        if (optionRepackStringHashes.IsSome(out var stringHashEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack string hashes: {stringHashEx}"));
        }
        
        // write string table
        var optionRepackStringTable = RepackStringTable(outStream, ref header, stringTable);
        if (optionRepackStringTable.IsSome(out var stringTableEx))
        {
            return Result.Err<int>(new InvalidOperationException($"Failed to repack string table: {stringTableEx}"));
        }
        
        // write header
        outStream.Seek(0, SeekOrigin.Begin);
        header.Write(outStream);
        
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
            
            var optionXInstance = instance.ToXElement(stream, types, stringHashes);
            if (!optionXInstance.IsSome(out var xInstance))
                return Option.Create(xe);
            
            xe.Add(xInstance);
        }
        
        return Option.Create(xe);
    }


    public Result<Dictionary<uint, string>, Exception> RepackStringHashes(XElement xe)
    {
        var hashMembers = xe.Descendants(AdfV04MemberLibrary.XName)
            .Where(xc => xc.GetAttribute("type")
                .MapOr(sh => sh.Contains("stringhash", StringComparison.InvariantCultureIgnoreCase), false))
            .ToArray();

        var stringHashes = new Dictionary<uint, string>();
        foreach (var hashMember in hashMembers)
        {
            uint hash = 0;
            var value = hashMember.Value;
            
            if (hashMember.GetAttribute("hash").IsSome(out _))
            {
                if (uint.TryParse(hashMember.Value, NumberStyles.HexNumber, null, out var intValue))
                {
                    hash = intValue;
                    value = intValue.ToString();
                }
                else
                {
                    hash = 0;
                    value = 0.ToString();
                }
            }
            else
            {
                hash = hashMember.Value.Jenkins();
            }
            
            stringHashes.TryAdd(hash, value);
        }
        
        return Result.OkExn(stringHashes);
    }
    
    public Result<string[], Exception> StringTableFromElement(XElement xe)
    {
        var namedElements = xe.Descendants()
            .Where(xc => xc.Name.LocalName != "string_hash")
            .Select(xc => xc.GetAttribute("name").MapOr(a => a, "DEADBEEF"))
            .Where(s => !uint.TryParse(s, out _))
            .Distinct()
            .ToArray();

        return Result.OkExn(namedElements);
    }
    
    public Result<AdfV04Type[], Exception> TypesFromElement(XElement xe)
    {
        var typeElements = xe.Descendants(AdfV04TypeLibrary.XName)
            .ToArray();

        var types = new List<AdfV04Type>();
        foreach (var typeElement in typeElements)
        {
            var resultType = typeElement.ToAdfV04Type();
            if (resultType.Err().IsSome(out var exception))
            {
                return Result.Err<AdfV04Type[]>(exception);
            }

            if (resultType.Ok().IsSome(out var adfType))
            {
                types.Add(adfType);
            }
        }
        
        return Result.OkExn(types.ToArray());
    }

    public Option<Exception> ReindexTypeNameIndices(ref AdfV04Type[] adfTypes, string[] stringTable)
    {
        foreach (var adfType in adfTypes)
        {
            var foundIndex = Array.FindIndex(stringTable, s => s.Equals(adfType.Name, StringComparison.InvariantCultureIgnoreCase));
            if (foundIndex < 0)
            {
                return (new Exception($"could not find {adfType.SafeName} in string table")).AsOption();
            }
            
            adfType.NameIndex = (ulong) foundIndex;
        }
        
        return Option<Exception>.None;
    }

    public Option<Exception> RepackTypes(Stream stream, ref AdfV04Header header, AdfV04Type[] adfTypes)
    {
        header.TypeOffset = (uint) stream.Position;
        header.TypeCount = (uint) adfTypes.Length;
        
        var builtinTypes = CreateInbuiltTypes().Select(t => t.TypeHash);
        var customTypes = adfTypes.Where(t => !builtinTypes.Contains(t.TypeHash)).ToList();
        
        foreach (var adfType in customTypes)
        {
            stream.Write(adfType.Type);
            stream.Write(adfType.Size);
            stream.Write(adfType.Alignment);
            stream.Write(adfType.TypeHash);
            stream.Write(adfType.NameIndex);
            stream.Write(adfType.Flags);
            stream.Write(adfType.ScalarType);
            stream.Write(adfType.ScalarTypeHash);
            stream.Write(adfType.BitCountOrArrayLength);
            stream.Write(adfType.MemberCountOrDataAlign);
        }
        
        return Option<Exception>.None;
    }
    
    public Option<Exception> RepackStringHashes(Stream stream, ref AdfV04Header header, Dictionary<uint,string> stringHashes)
    {
        header.StringHashOffset = (uint) stream.Position;
        header.StringHashCount = (uint) stringHashes.Count;

        foreach (var kvp in stringHashes)
        {
            stream.Write(Encoding.UTF8.GetBytes(kvp.Value));
            stream.Write((byte) 0x00);
            stream.Write((ulong) kvp.Key);
        }
        
        return Option<Exception>.None;
    }
    
    public Option<Exception> RepackStringTable(Stream stream, ref AdfV04Header header, string[] stringTable)
    {
        header.StringTableOffset = (uint) stream.Position;
        header.StringTableCount = (uint) stringTable.Length;

        foreach (var stringEntry in stringTable)
        {
            stream.Write((byte) (stringEntry.Length + 1));
        }
        
        foreach (var stringEntry in stringTable)
        {
            stream.Write(Encoding.UTF8.GetBytes(stringEntry));
            stream.Write((byte) 0x00);
        }
        
        return Option<Exception>.None;
    }

    public Result<AdfV04Instance[], Exception> FromXElement(XElement xe, string[] stringTable, AdfV04Type[] types)
    {
        var instanceElements = xe.Descendants(AdfV04InstanceLibrary.XName)
            .ToArray();

        var instances = new List<AdfV04Instance>();
        foreach (var instanceElement in instanceElements)
        {
            if (!instanceElement.GetAttribute("name").IsSome(out var name))
            {
                return Result.Err<AdfV04Instance[]>(new InvalidOperationException("Missing name attribute"));
            }

            var resultTypeHash = instanceElement.GetAdfV04Type(types);
            if (!resultTypeHash.IsOk(out var adfType))
            {
                return Result.Err<AdfV04Instance[]>(new InvalidOperationException("Invalid type hash"));
            }
            
            var instance = new AdfV04Instance
            {
                Name = name,
                NameHash = name.HashJenkins(),
                TypeHash = adfType.TypeHash
            };
            
            var foundIndex = Array.FindIndex(stringTable, s => s.Equals(instance.Name, StringComparison.InvariantCultureIgnoreCase));
            if (foundIndex < 0)
            {
                return Result.Err<AdfV04Instance[]>(new InvalidOperationException($"could not find {instance.Name} in string table"));
            }
            
            instance.NameIndex = (ulong) foundIndex;
            instances.Add(instance);
        }
        
        return Result.OkExn(instances.ToArray());
    }
    
    public Result<AdfV04Instance[], Exception> RepackInstances(XElement xe, Stream stream, ref AdfV04Header header,
        ref AdfV04Instance[] instances, Dictionary<uint, string> stringHashes, string[] stringTable, AdfV04Type[] types)
    {
        var instanceElements = xe.Descendants(AdfV04InstanceLibrary.XName)
            .ToArray();

        if (instances.Length != instanceElements.Length)
        {
            return Result.Err<AdfV04Instance[]>(new InvalidOperationException($"instanceElements.Length != instances.Length"));
        }
        
        var instancesXe = instances.Zip(instanceElements, (instance, xInstance) => new
        {
            Instance = instance,
            XInstance = xInstance
        });
        
        // var contentOffset = (int) (instances.Select(inst => inst.PayloadSize).Aggregate((a, b) => a + b));
        // using var memoryStream = new MemoryStream();

        for (var i = 0; i < instances.Length; i++)
        {
            var instance = instances[i];
            var xInstance = instanceElements[i];
            
            var resultAdfType = instance.GetType(types);
            if (!resultAdfType.IsOk(out var adfType))
            {
                return Result.Err<AdfV04Instance[]>(new InvalidOperationException($"{instance.Name} type was missing from type list"));
            }
            
            // content offset is relative to instance offset
            // use memory stream to avoid extra functions and data passing
            var contentOffset = (int) instance.PayloadSize;
            using var memoryStream = new MemoryStream();
            
            // write data
            var optionException = AdfV04InstanceLibrary.FromXElement(xInstance, memoryStream, stringHashes, stringTable, types, ref contentOffset);
            if (optionException.IsSome(out var exception))
            {
                return Result.Err<AdfV04Instance[]>(exception);
            }
            
            instances[i].PayloadOffset = (uint) stream.Position;
            instances[i].PayloadSize = (uint) memoryStream.Length;
            
            memoryStream.CopyTo(stream);
        }
        
        header.InstanceOffset = (uint) stream.Position;
        header.InstanceCount = (uint) instances.Length;

        foreach (var instance in instances)
        {
            stream.Write(instance.NameHash);
            stream.Write(instance.TypeHash);
            stream.Write(instance.PayloadOffset);
            stream.Write(instance.PayloadSize);
            stream.Write(instance.NameIndex);
        }
        
        return Result.OkExn(instances.ToArray());
    }
}

public static class AdfV04FileLibrary
{
    public const string XName = "adf";
    public const int Version = 4;

    public static readonly string VersionName = $"{XName.ToUpper()} v{Version:D2}";
}
