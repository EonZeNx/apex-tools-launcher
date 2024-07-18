using ATL.Core.Libraries;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Dialogs;

public class ProfileDialogResult
{
    public string Title { get; set; } = "";
    public bool Delete { get; set; } = false;
}

public partial class AddEditProfileDialog : ComponentBase
{
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [CascadingParameter]
    public MudDialogInstance? MudDialog { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public bool Edit { get; set; } = false;

    public ProfileDialogResult Result { get; set; } = new();
    protected bool ResultValid { get; set; } = true;

    protected void CheckResultValid(string value)
    {
        Result.Title = value;

        if (string.IsNullOrEmpty(value))
        {
            ResultValid = false;
            return;
        }

        var profileConfigs = ProfileConfigService.GetGame(GameId);
        ResultValid = !profileConfigs.ContainsKey(value);
    }
    
    protected void ReloadData()
    {
        if (ConstantsLibrary.IsStringInvalid(ProfileId))
        { // create profile
            return;
        }

        var profileConfig = ProfileConfigService.Get(GameId, ProfileId);
        Result.Title = profileConfig.Title;
    }
    
    protected override void OnParametersSet()
    {
        ReloadData();
    }
    
    protected void Close() => MudDialog?.Close(DialogResult.Cancel());
    protected void Confirm() => MudDialog?.Close(Result);

    protected void Delete()
    {
        Result.Delete = true;
        MudDialog?.Close(Result);
    }
}