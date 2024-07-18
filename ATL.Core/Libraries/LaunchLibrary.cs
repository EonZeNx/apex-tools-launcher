using System.Diagnostics;
using ATL.Core.Config;
using ATL.Core.Config.GUI;

namespace ATL.Core.Libraries;

public class LaunchLibrary
{
    public string GameId { get; set; } = "";
    
    public AppConfig AppConfig { get; set; } = new();
    public GameConfig GameConfig { get; set; } = new();
    public ProfileConfig ProfileConfig { get; set; } = new();
    public Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    public void LaunchGame()
    {
        var modLaunchArguments = SetupMods();
        var profileLaunchArguments = ConfigLibrary.GetLaunchArguments(GameConfig, ProfileConfig);

        var targetExe = "steam.exe";
        var targetPath = AppConfig.SteamPath;
        List<string> launchArguments = ["-applaunch", GameConfig.SteamAppId];
        
        if (ConstantsLibrary.IsStringInvalid(GameConfig.SteamAppId))
        { // non-steam game
            targetExe = Path.GetFileName(GameConfig.Path);
            targetPath = Path.GetDirectoryName(GameConfig.Path);
            launchArguments = [];
        }

        launchArguments.AddRange(modLaunchArguments);
        launchArguments.AddRange(profileLaunchArguments);
        
        var startInfo = new ProcessStartInfo(targetExe, string.Join(' ', launchArguments))
        {
            WorkingDirectory = targetPath,
            UseShellExecute = true
        };

        var process = new Process();
        process.StartInfo = startInfo;

        try
        {
            process.Start();
        }
        catch
        {
            return;
        }
    }
    
    public List<string> SetupMods()
    {
        var vfsLaunchArguments = new List<string>();

        var installPath = (new DirectoryInfo(GameConfig.Path).Parent ?? new DirectoryInfo(@".\")).FullName;
        if (!Directory.Exists(installPath))
        {
            return [];
        }

        // Shouldn't need cleaning thanks to vfs
        var vfsFsPath = Path.Join(installPath, AppConfig.VfsFsPath);
        if (!Directory.Exists(vfsFsPath))
        {
            Directory.CreateDirectory(vfsFsPath);
        }
        
        foreach (var (modId, modVersion) in ProfileConfig.ModConfigs)
        {
            if (!ModConfigs.TryGetValue(modId, out var modConfig))
            {
                continue;
            }

            if (!modConfig.Versions.ContainsKey(modVersion))
            {
                continue;
            }

            switch (modConfig.Type)
            {
            case ModType.VfsArchive:
                SetupVfsArchiveMod(modId, modConfig, modVersion, GameId);
                break;
            case ModType.Dll:
                SetupDllMod(modId, modConfig, modVersion, GameId);
                break;
            case ModType.VfsFile:
            default:
                vfsLaunchArguments.Add(SetupVfsFileMod(modId, modVersion, GameId, vfsFsPath));
                break;
            }
        }
        
        // Install mods
        // - VFS/Archive mods should create a symlink and add to launch arguments
        // - DLL mods should copy into the `mods` directory

        return vfsLaunchArguments;
    }
    
    public string SetupVfsFileMod(string modId, string version, string gameId, string vfsFsPath)
    {
        var modConfigPath = ConfigLibrary.GetModConfigPath(gameId);
        var modVersionTarget = ConfigLibrary.GetModTargetPath(modId, version);
        var modSymlinkTarget = Path.Join(modConfigPath, modVersionTarget);

        var modSymlinkInstallName = $"{modId}_{version}";
        var modInstallPath = Path.Join(vfsFsPath, modSymlinkInstallName);
        if (!Directory.Exists(modInstallPath))
        {
            Directory.CreateSymbolicLink(modInstallPath, modSymlinkTarget);
        }

        var modLaunchTargetPath = Path.Join(AppConfig.VfsFsPath, modSymlinkInstallName);
        var modLaunchArg = $"--vfs-fs {modLaunchTargetPath}";
        
        return modLaunchArg;
    }
    
    public void SetupVfsArchiveMod(string modId, ModConfig config, string version, string gameId)
    {
        
    }
    
    public void SetupDllMod(string modId, ModConfig config, string version, string gameId)
    {
        
    }
}