using System.Xml;
using System.Xml.Linq;
using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;

namespace ApexFormat.IC.V01;

public class IcV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.Read<IcV01Instance>().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        var file = new IcV01File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        List<IcV01Instance> instances = [];
        while (inBuffer.Position < inBuffer.Length)
        {
            var optionInstance = inBuffer.Read<IcV01Instance>();
            if (!optionInstance.IsSome(out var instance))
            {
                break;
            }
            
            instances.Add(instance);
        }
        
        var outer = new XElement("instances");
        outer.SetAttributeValue("extension", "bin");

        foreach (var instance in instances)
        {
            outer.Add(instance.ToXElement());
        }
        
        var xd = new XDocument(XDocumentLibrary.ProjectComment(), outer);
        using var xw = XmlWriter.Create(outBuffer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        xd.Save(xw);

        return 0;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new IcV01File();

        var result = -1;
        if (file.CanExtractPath(inFilePath))
        {
            var extractResult = file.ExtractPathToPath(inFilePath, outDirectory);
            extractResult.IsOk(out result);
        }
        else if (file.CanRepackPath(inFilePath))
        {
            result = file.RepackPathToPath(inFilePath, outDirectory);
        }
        
        return result;
    }

    public string GetProcessorName()
    {
        return "IC v01";
    }
}