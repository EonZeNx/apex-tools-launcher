using ApexFormat.ADF.V04;
using ATL.Core.Class;

namespace ATL.Test;

public class AdfV04Tests
{
    public static string SuccessPath { get; set; } = @".\Tests\ADFv04\w011_pistol_u_pozhar_98.wtunec";
    public static string FailPath { get; set; } = @".\Tests\fail.txt";
    public static string OutDirectory { get; set; } = @".\Tests\out";
    
    [SetUp]
    public void Setup()
    {
        DotEnv.Load();

        var envSuccessPath = Environment.GetEnvironmentVariable("TEST_PATH_ADF_V04_SUCCESS");
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
        var result = AdfV04Manager.CanProcess(SuccessPath);
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanProcessFail()
    {
        var result = AdfV04Manager.CanProcess(FailPath);
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void DecompressSuccess()
    {
        var manager = new AdfV04Manager();
        var result = manager.ProcessBasic(SuccessPath, OutDirectory);
        
        Assert.That(result, Is.AtLeast(0));
    }
}