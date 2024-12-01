using ApexFormat.RTPC.V01;
using ApexFormat.RTPC.V01.Class;
using ApexFormat.TAB.V02;
using ApexToolsLauncher.Core.Class;

namespace ApexToolsLauncher.Test;

public class RtpcV01Tests
{
    public static string SuccessPath { get; set; } = @"D:\Projects\Various\pysharp\test\dlc3_satellite_base_01.blo";
    public static string FailPath { get; set; } = @".\Tests\fail.txt";
    public static string OutDirectory { get; set; } = @".\Tests\out";
    
    [SetUp]
    public void Setup()
    {
        DotEnv.Load();

        var envSuccessPath = Environment.GetEnvironmentVariable("TEST_PATH_RTPC_V01_SUCCESS");
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
    public void FilterBySuccess()
    {
        var inBuffer = new FileStream(SuccessPath, FileMode.Open);
        var optionHeader = inBuffer.ReadRtpcV01Header();
        if (!optionHeader.IsSome(out var header))
        {
            Assert.Fail();
            return;
        }
        
        var optionContainer = inBuffer.ReadRtpcV01Container();
        if (!optionContainer.IsSome(out var container))
        {
            Assert.Fail();
            return;
        }

        var rigidObject = new RtpcV01FilterString("_class", "CRigidObject")
        {
            SubFilters = [
                new RtpcV01Filter("filename"),
                new RtpcV01Filter("world")
            ]
        };

        var effectPointEmitter = new RtpcV01FilterString("_class", "CEffectPointEmitter")
        {
            SubFilters = [
                new RtpcV01Filter("effect"),
                new RtpcV01Filter("world")
            ]
        };

        var dynamicLightObject = new RtpcV01FilterString("_class", "CDynamicLightObject")
        {
            SubFilters = [
                new RtpcV01Filter("diffuse"),
                new RtpcV01Filter("falloff_start"),
                new RtpcV01Filter("is_spot_light"),
                new RtpcV01Filter("multiplier"),
                new RtpcV01Filter("name"),
                new RtpcV01Filter("on_during_daytime"),
                new RtpcV01Filter("projected_texture"),
                new RtpcV01Filter("projected_texture_enabled"),
                new RtpcV01Filter("projected_texture_u_scale"),
                new RtpcV01Filter("projected_texture_v_scale"),
                new RtpcV01Filter("radius"),
                new RtpcV01Filter("spot_angle"),
                new RtpcV01Filter("spot_inner_angle"),
                new RtpcV01Filter("volume_intensity"),
                new RtpcV01Filter("volumetric_mode"),
                new RtpcV01Filter("world"),
            ]
        };

        var staticDecalObject = new RtpcV01FilterString("_class", "CStaticDecalObject")
        {
            SubFilters = [
                new RtpcV01Filter("Emissive"),
                new RtpcV01Filter("alpha_max"),
                new RtpcV01Filter("alpha_min"),
                new RtpcV01Filter("alphamask_offset_u"),
                new RtpcV01Filter("alphamask_offset_v"),
                new RtpcV01Filter("alphamask_source_channel"),
                new RtpcV01Filter("alphamask_strength"),
                new RtpcV01Filter("alphamask_texture"),
                new RtpcV01Filter("alphamask_tile_u"),
                new RtpcV01Filter("alphamask_tile_v"),
                new RtpcV01Filter("color"),
                new RtpcV01Filter("diffuse_texture"),
                new RtpcV01Filter("distance_field_decal_mpm"),
                new RtpcV01Filter("is_distance_field_stencil"),
                new RtpcV01Filter("offset_u"),
                new RtpcV01Filter("offset_v"),
                new RtpcV01Filter("tile_u"),
                new RtpcV01Filter("tile_v"),
                new RtpcV01Filter("world"),
            ]
        };

        var filters = new IRtpcV01Filter[] {
            rigidObject,
            effectPointEmitter,
            dynamicLightObject,
            staticDecalObject
        };

        var originalLength = container.ContainerCount;
        container.FilterBy(filters);
        var filteredLength = container.ContainerCount;
        
        Assert.That(filteredLength, Is.AtMost(originalLength));
    }
}