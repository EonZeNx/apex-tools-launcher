using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Dialogs;

public partial class CreateModContentDialog : ComponentBase
{
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public MudDialogInstance? MudDialog { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ModId { get; set; } = "ModId";
    
    protected string ModContentId { get; set; } = "";
    
    protected ModContentConfig ModContentConfig { get; set; } = new()
    {
        Title = "",
        Description = "",
        Target = "",
    };

    protected bool ContentIdValid()
    {
        var result = ModContentId.Length != 0;
        if (result)
        {
            // result &= !ModConfigService.ContentExists(GameId, ModId, ModContentId);
        }
        
        return result;
    }

    protected bool TitleValid()
    {
        var result = ModContentConfig.Title.Length != 0;
        
        return result;
    }

    protected bool DialogValid()
    {
        var result = ContentIdValid();
        result &= TitleValid();
        
        return result;
    }
    
    protected void Close() => MudDialog?.Close(DialogResult.Cancel());

    protected void Confirm()
    {
        var modConfig = ModConfigService.Get(GameId, ModId);
        modConfig.Versions[ModContentId] = ModContentConfig;
        
        // ModConfigService.Save(GameId, ModId, modConfig);
        MudDialog?.Close();
    }
}
