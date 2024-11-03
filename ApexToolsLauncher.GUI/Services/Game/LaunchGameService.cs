using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Mod;

namespace ApexToolsLauncher.GUI.Services.Game;

public class LaunchGameService : ILaunchGameService
{
    protected AppConfigService AppConfigService { get; set; }
    protected IAppStateService AppStateService { get; set; }
    protected IGameConfigService GameConfigService { get; set; }
    protected IProfileConfigService ProfileConfigService { get; set; }
    protected IModConfigService ModConfigService { get; set; }

    public LaunchGameService(AppConfigService appConfigService, IAppStateService appStateService, IGameConfigService gameConfigService, IProfileConfigService profileConfigService, IModConfigService modConfigService)
    {
        AppConfigService = appConfigService;
        AppStateService = appStateService;
        GameConfigService = gameConfigService;
        ProfileConfigService = profileConfigService;
        ModConfigService = modConfigService;
    }
    
    public void Launch(string gameId)
    {
        var appConfig = AppConfigService.Get();
        var gameConfig = GameConfigService.Get(gameId);
        var profileConfig = ProfileConfigService.Get(gameId, AppStateService.GetLastProfileId(gameId));
        var modConfigs = ModConfigService.GetAllFromGame(gameId);
        
        var gameLauncher = new GameLauncher
        {
            GameId = gameId,
            AppConfig = appConfig,
            GameConfig = gameConfig,
            ProfileConfig = profileConfig,
            ModConfigs = modConfigs
        };
        
        gameLauncher.Start();
    }
}