using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Dialogs;

public partial class CreateModDialog : ComponentBase
{
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public MudDialogInstance? MudDialog { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    protected string ModId { get; set; } = "";
    
    protected ModConfig ModConfig { get; set; } = new()
    {
        Title = "",
        Description = "",
        Type = ModType.VfsFile
    };

    protected bool ModIdValid()
    {
        var result = ModId.Length != 0;
        if (result)
        {
            // result &= !ModConfigService.Exists(GameId, ModId);
        }
        
        return result;
    }

    protected bool TitleValid()
    {
        var result = ModConfig.Title.Length != 0;
        
        return result;
    }

    protected bool DialogValid()
    {
        var result = ModIdValid();
        result &= TitleValid();
        
        return result;
    }
    
    protected void Close() => MudDialog?.Close(DialogResult.Cancel());

    protected void Confirm()
    {
        // ModConfigService.Save(GameId, ModId, ModConfig);
        MudDialog?.Close();
    }
}
