using ApexFormat.SARC.V02;
using ApexToolsLauncher.Core.Class;

namespace ApexToolsLauncher.Test;

public class SarcV02Tests
{
    public static string SuccessPath { get; set; } = @".\Tests\SARCv02\v0604.sarc";
    public static string FailPath { get; set; } = @".\Tests\fail.txt";
    public static string OutDirectory { get; set; } = @".\Tests\out";

    [SetUp]
    public void Setup()
    {
        DotEnv.Load();

        var envSuccessPath = Environment.GetEnvironmentVariable("TEST_PATH_SARC_V02_SUCCESS");
        if (File.Exists(envSuccessPath))
            SuccessPath = envSuccessPath;
        
        var envFailPath = Environment.GetEnvironmentVariable("TEST_PATH_FAIL");
        if (File.Exists(envFailPath))
            FailPath = envFailPath;
        
        var envOutDirectory = Environment.GetEnvironmentVariable("TEST_PATH_OUT_DIRECTORY");
        if (File.Exists(envOutDirectory))
            OutDirectory = envOutDirectory;
    }
    
    [Test]
    public void CanProcessSuccess()
    {
        var result = SarcV02Manager.CanProcess(SuccessPath);
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanProcessFail()
    {
        var result = SarcV02Manager.CanProcess(FailPath);
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void DecompressSuccess()
    {
        var manager = new SarcV02Manager();
        var result = manager.ProcessBasic(SuccessPath, OutDirectory);
        
        Assert.That(result, Is.AtLeast(0));
    }
}