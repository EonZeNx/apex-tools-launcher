using System.Numerics;
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
    
    protected float Pitch { get; set; }
    protected float Psi => float.DegreesToRadians(Pitch);
    
    protected float Yaw { get; set; }
    protected float Theta => float.DegreesToRadians(Yaw);
    
    protected float Roll { get; set; }
    protected float Phi => float.DegreesToRadians(Roll);
    
    protected Vector3 Output { get; set; }
    
    protected void Calculate()
    {
        if (LogService is null) return;

        var outA = Vector3.Zero;
        outA.X = (float) (Math.Cos(Theta) * Math.Cos(Phi));
        outA.Y = (float) (Math.Sin(Psi) * Math.Sin(Theta) * Math.Cos(Phi) - Math.Cos(Psi) * Math.Sin(Phi));
        outA.Z = (float) (Math.Cos(Psi) * Math.Sin(Theta) * Math.Cos(Phi) + Math.Sin(Psi) * Math.Sin(Phi));

        var outB = Vector3.Zero;
        outB.X = (float) (Math.Cos(Theta) * Math.Sin(Phi));
        outB.Y = (float) (Math.Sin(Psi) * Math.Sin(Theta) * Math.Sin(Phi) - Math.Cos(Psi) * Math.Cos(Phi));
        outB.Z = (float) (Math.Cos(Psi) * Math.Sin(Theta) * Math.Sin(Phi) + Math.Sin(Psi) * Math.Cos(Phi));

        var outC = Vector3.Zero;
        outC.X = (float) -Math.Sin(Theta);
        outC.Y = (float) (Math.Sin(Psi) * Math.Cos(Theta));
        outC.Z = (float) (Math.Cos(Psi) * Math.Cos(Theta));
    }

    protected static string Format(float value) => $"{value:0.####}";
}