using ATL.Core.Libraries;

namespace ATL.GUI.Services;

public class LaunchGameService
{
    protected AppConfigService AppConfigService { get; set; }
    protected GameConfigService GameConfigService { get; set; }
    protected ProfileConfigService ProfileConfigService { get; set; }
    protected ModConfigService ModConfigService { get; set; }

    public LaunchGameService(AppConfigService appConfigService, GameConfigService gameConfigService, ProfileConfigService profileConfigService, ModConfigService modConfigService)
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
        var profileConfig = ProfileConfigService.Get(gameId, gameConfig.SelectedProfile);
        var modConfigs = ModConfigService.GetGame(gameId);
        
        var launch = new LaunchLibrary
        {
            GameId = gameId,
            AppConfig = appConfig,
            GameConfig = gameConfig,
            ProfileConfig = profileConfig,
            ModConfigs = modConfigs
        };
        
        launch.LaunchGame();
    }
}