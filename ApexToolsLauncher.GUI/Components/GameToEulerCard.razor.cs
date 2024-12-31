using System.Numerics;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.Development;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class GameToEulerCard : MudComponentBase
{
    [Inject]
    protected ILogService? LogService { get; set; }

    protected Matrix3x3 Input = Matrix3x3.Zero();
    
    protected float Psi { get; set; } = 0;
    protected float Pitch => float.RadiansToDegrees(Psi);
    
    protected float Theta { get; set; } = 0;
    protected float Yaw => float.RadiansToDegrees(Theta);

    protected float Phi { get; set; } = 0;
    protected float Roll => float.RadiansToDegrees(Phi);
    
    protected string Calculate()
    {
        if (LogService is null) return "";

        Psi = 0;
        Theta = 0;
        Phi = 0;
        
        if (!Input.C.X.AlmostEqual(1) && !Input.C.X.AlmostEqual(-1))
        {
            Theta = (float) -Math.Asin(Input.C.X);
            Psi = (float) (Math.Atan2(Input.C.Y / Math.Cos(Theta), Input.C.Z / Math.Cos(Theta)));
            Phi = (float) (Math.Atan2(Input.B.X / Math.Cos(Theta), Input.A.X / Math.Cos(Theta)));
        }
        else if (Input.C.X.AlmostEqual(1))
        {
            Theta = (float) -Math.PI / 2;
            Psi = (float) (-Phi + Math.Atan2(-Input.A.Y, -Input.A.Z));
        }
        else if (Input.C.X.AlmostEqual(-1))
        {
            Theta = (float) Math.PI / 2;
            Psi = (float) (Phi + Math.Atan2(Input.A.Y, Input.A.Z));
        }

        return $"{Format(Pitch)}, {Format(Yaw)}, {Format(Roll)}";
    }

    protected static string Format(float value) => $"{value:0.###}";
}