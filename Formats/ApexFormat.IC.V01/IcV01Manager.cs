using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.IC.V01;

public class IcV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadIcV01Instance().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        if (Directory.Exists(path))
        { // don't support repacking directories just yet
            return false;
        }
        
        if (File.Exists(path))
        {
            using var fileStream = new FileStream(path, FileMode.Open);
            return CanProcess(fileStream);
        }

        return false;
    }
    
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        List<IcV01Instance> Instances = [];
        while (inBuffer.Position < inBuffer.Length)
        {
            var optionInstance = inBuffer.ReadIcV01Instance();
            if (!optionInstance.IsSome(out var instance))
            {
                break;
            }
            
            Instances.Add(instance);
        }
        
        // todo: extract to xml

        return 0;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        using var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileName = Path.GetFileNameWithoutExtension(inFilePath);
        var xmlFilePath = Path.Join(outDirectoryPath, $"{fileName}.xml");
        
        using var outBuffer = new FileStream(xmlFilePath, FileMode.Create);
        var result = Decompress(inBuffer, outBuffer);
        
        return result;
    }
}