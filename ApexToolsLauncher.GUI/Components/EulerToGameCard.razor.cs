using System.Numerics;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.Development;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class EulerToGameCard : MudComponentBase
{
    [Inject]
    protected ILogService? LogService { get; set; }
    
    protected float Pitch { get; set; }
    protected float Psi => float.DegreesToRadians(Pitch);
    
    protected float Yaw { get; set; }
    protected float Theta => float.DegreesToRadians(Yaw);
    
    protected float Roll { get; set; }
    protected float Phi => float.DegreesToRadians(Roll);
    
    protected Matrix3x3 Output { get; set; }
    protected string OutputFormated =>
        $"{Format(Output.A.X)}, {Format(Output.B.X)}, {Format(Output.C.X)}, 0, {Format(Output.A.Y)}, {Format(Output.B.Y)}, {Format(Output.C.Y)}, 0, {Format(Output.A.Z)}, {Format(Output.B.Z)}, {Format(Output.C.Z)}, 0";

    protected string Calculate()
    {
        if (LogService is null) return "fail";

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
        
        Output = new Matrix3x3
        {
            A = outA,
            B = outB,
            C = outC
        };
        
        return OutputFormated;
    }

    protected static string Format(float value) => $"{value:0.#####}";
}