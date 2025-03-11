using System.Xml;
using System.Xml.Linq;
using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using RustyOptions;

namespace ApexFormat.IC.V01;

public class IcV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "bin";
    
    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !fileStream.Read<IcV01Instance>().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
    }

    public Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        List<IcV01Instance> instances = [];
        while (inStream.Position < inStream.Length)
        {
            var optionInstance = inStream.Read<IcV01Instance>();
            if (!optionInstance.IsSome(out var instance))
            {
                break;
            }
            
            instances.Add(instance);
        }
        
        var xd = XProjectBuilder.Create()
            .WithType(IcV01FileLibrary.XName)
            .WithVersion(IcV01FileLibrary.Version.ToString())
            .WithExtension(ExtractExtension)
            .WithChildren(instances, inst => inst.ToXElement())
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

            if (!string.Equals(projectType, IcV01FileLibrary.XName))
                return false;
            
            if (!project.Version.IsSome(out var projectVersionString))
                return false;

            if (!int.TryParse(projectVersionString, out var projectVersion))
                return false;

            if (projectVersion != IcV01FileLibrary.Version)
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

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        var xe = XElement.Load(inStream);
        
        var optionType = xe.GetAttribute("type");
        if (!optionType.IsSome(out var xeType))
            return Result.Err<int>(new InvalidOperationException("Missing type attribute"));
        
        if (!string.Equals(xeType, IcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element type is {xeType} not {IcV01FileLibrary.XName}"));
        }

        var optionExtension = xe.GetAttribute("extension");
        if (optionExtension.IsSome(out var extension))
        {
            ExtractExtension = extension;
        }

        var xInstanceArray = xe.Elements(IcV01InstanceLibrary.XName).ToArray();
        foreach (var xi in xInstanceArray)
        {
            var instanceResult = xi.Read<IcV01Instance>();
            if (instanceResult.IsErr(out _))
            {
                return instanceResult.Map(_ => -1);
            }

            outStream.Write(instanceResult.Unwrap());
        }

        return Result.OkExn(0);
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        var xe = XElement.Load(inPath);
        
        var optionType = xe.GetAttribute("type");
        if (!optionType.IsSome(out var xeType))
            return Result.Err<int>(new InvalidOperationException($"Missing type attribute"));
        
        if (!string.Equals(xeType, IcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element type is {xeType} not {IcV01FileLibrary.XName}"));
        }

        var optionExtension = xe.GetAttribute("extension");
        if (optionExtension.IsSome(out var extension))
        {
            ExtractExtension = extension;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(inPath);
        var repackFilePath = Path.Join(outPath, $"{fileName}.{ExtractExtension}");
        
        using var inStream = new FileStream(inPath, FileMode.Open);
        using var outStream = new FileStream(repackFilePath, FileMode.Create);
        
        return RepackStreamToStream(inStream, outStream);
    }
}

public static class IcV01FileLibrary
{
    public const string XName = "ic";
    public const int Version = 1;

    public static readonly string VersionName = $"{XName.ToUpper()} v{Version:D2}";
}
