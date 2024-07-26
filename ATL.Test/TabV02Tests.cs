using ApexFormat.TAB.V02;
using ATL.Core.Class;

namespace ATL.Test;

public class TabV02Tests
{
    public static string SuccessPath { get; set; } = @".\Tests\TABv02\game67.tab";
    public static string FailPath { get; set; } = @".\Tests\fail.txt";
    public static string OutDirectory { get; set; } = @".\Tests\out";
    
    [SetUp]
    public void Setup()
    {
        DotEnv.Load();

        var envSuccessPath = Environment.GetEnvironmentVariable("TEST_PATH_TAB_V02_SUCCESS");
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
        var result = TabV02Manager.CanProcess(SuccessPath);
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanProcessFail()
    {
        var result = TabV02Manager.CanProcess(FailPath);
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void DecompressSuccess()
    {
        var manager = new TabV02Manager();
        var result = manager.ProcessBasic(SuccessPath, OutDirectory);
        
        Assert.That(result, Is.AtLeast(0));
    }
}