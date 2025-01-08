using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;

namespace ApexToolsLauncher.GUI.Services;

public class LaunchGameService
{
    protected AppConfigService AppConfigService { get; set; }
    protected IGameConfigService GameConfigService { get; set; }
    protected IProfileConfigService ProfileConfigService { get; set; }
    protected IModConfigService ModConfigService { get; set; }

    public LaunchGameService(AppConfigService appConfigService, IGameConfigService gameConfigService, IProfileConfigService profileConfigService, IModConfigService modConfigService)
    {
        AppConfigService = appConfigService;
        GameConfigService = gameConfigService;
        ProfileConfigService = profileConfigService;
        ModConfigService = modConfigService;
    }
    
    public void Launch(string gameId)
    {
        var appConfig = AppConfigService.Get();
        var gameConfig = GameConfigService.Get(gameId);
        // var profileConfig = ProfileConfigService.Get(gameId, gameConfig.SelectedProfile);
        // var modConfigs = ModConfigService.GetAllFromGame(gameId);
        //
        // var launch = new LaunchLibrary
        // {
        //     GameId = gameId,
        //     AppConfig = appConfig,
        //     GameConfig = gameConfig,
        //     ProfileConfig = profileConfig,
        //     ModConfigs = modConfigs
        // };
        //
        // launch.LaunchGame();
    }
}