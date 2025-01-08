using System.Numerics;
using ApexToolsLauncher.GUI.Services.Development;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using RustyOptions;

namespace ApexToolsLauncher.GUI.Components;


public struct Matrix3x3
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    public Matrix3x3(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
    }
    
    public Matrix3x3(float value)
    {
        A = new Vector3(value);
        B = new Vector3(value);
        C = new Vector3(value);
    }

    public static Matrix3x3 Zero()
    {
        return new Matrix3x3(0);
    }

    public static Matrix3x3 One()
    {
        return new Matrix3x3(1);
    }

    public static Matrix3x3 Identity()
    {
        var a = new Vector3(1, 0, 0);
        var b = new Vector3(0, 1, 0);
        var c = new Vector3(0, 0, 1);
        
        return new Matrix3x3(a, b, c);
    }


    public Vector3 FromX() => new (A.X, B.X, C.X);
    public Vector3 FromY() => new (A.Y, B.Y, C.Y);
    public Vector3 FromZ() => new (A.Z, B.Z, C.Z);

    public override string ToString()
    {
        return $"({A}, {B}, {C})";
    }
}

public partial class EulerConvertComponent : MudComponentBase
{
    [Inject]
    protected ILogService? LogService { get; set; }
    
    protected float Pitch { get; set; }
    protected float Yaw { get; set; }
    protected float Roll { get; set; }
    protected Matrix3x3 Output { get; set; }
    protected string OutputFormated =>
        $"{Format(Output.A.X)}, {Format(Output.A.Y)}, {Format(Output.A.Z)}, 0, {Format(Output.B.X)}, {Format(Output.B.Y)}, {Format(Output.B.Z)}, 0, {Format(Output.C.X)}, {Format(Output.C.Y)}, {Format(Output.C.Z)}, 0";

    protected string Calculate()
    {
        if (LogService is null) return "fail";
        
        var rotator = new Vector3(Pitch, Yaw, Roll);

        var rx = ToRx(rotator);
        var ry = ToRy(rotator);
        var rz = ToRz(rotator);

        var rxRotator = Matrix3x3.Identity();
        
        var ryRotator = Matrix3x3.Zero();
        ryRotator.A = CalcRotatorVector(rx.A, rxRotator);
        ryRotator.B = CalcRotatorVector(rx.B, rxRotator);
        ryRotator.C = CalcRotatorVector(rx.C, rxRotator);
        
        var rzRotator = Matrix3x3.Zero();
        rzRotator.A = CalcRotatorVector(ry.A, ryRotator);
        rzRotator.B = CalcRotatorVector(ry.B, ryRotator);
        rzRotator.C = CalcRotatorVector(ry.C, ryRotator);
        
        var result = Matrix3x3.Zero();
        result.A = CalcRotatorVector(rz.A, rzRotator);
        result.B = CalcRotatorVector(rz.B, rzRotator);
        result.C = CalcRotatorVector(rz.C, rzRotator);

        var r1 = result.FromX();
        var r2 = result.FromY();
        var r3 = result.FromZ();

        Output = new Matrix3x3(r1, r2, r3);
        return OutputFormated;
    }


    protected Matrix3x3 ToRx(Vector3 vector)
    {
        var xRadians = float.DegreesToRadians(vector.X);
        var sinRadians = (float) Math.Sin(xRadians);
        var cosRadians = (float) Math.Cos(xRadians);
        
        var rx = Matrix3x3.Zero();
        
        rx.A.X = 1;
        
        rx.B.Y = cosRadians;
        rx.B.Z = -sinRadians;
        
        rx.C.Y = sinRadians;
        rx.C.Z = cosRadians;
        
        return rx;
    }

    protected Matrix3x3 ToRy(Vector3 vector)
    {
        var yRadians = float.DegreesToRadians(vector.Y);
        var sinRadians = (float) Math.Sin(yRadians);
        var cosRadians = (float) Math.Cos(yRadians);
        
        var ry = Matrix3x3.Zero();
        
        ry.A.X = cosRadians;
        ry.A.Z = sinRadians;
        
        ry.B.Y = 1;
        
        ry.C.X = -sinRadians;
        ry.C.Z = cosRadians;
        
        return ry;
    }

    protected Matrix3x3 ToRz(Vector3 vector)
    {
        var zRadians = float.DegreesToRadians(vector.Z);
        var sinRadians = (float) Math.Sin(zRadians);
        var cosRadians = (float) Math.Cos(zRadians);
        
        var rz = Matrix3x3.Zero();
        
        rz.A.X = cosRadians;
        rz.A.Y = -sinRadians;
        
        rz.B.X = sinRadians;
        rz.B.Y = cosRadians;
        
        rz.C.Z = 1;
        
        return rz;
    }


    protected float Sum(Vector3 vector)
    {
        return vector.X + vector.Y + vector.Z;
    }

    protected Vector3 GetXRotator(Matrix3x3 rotator)
    {
        var result = new Vector3(rotator.A.X, rotator.B.X, rotator.C.X);

        return result;
    }

    protected Vector3 GetYRotator(Matrix3x3 rotator)
    {
        var result = new Vector3(rotator.A.Y, rotator.B.Y, rotator.C.Y);

        return result;
    }

    protected Vector3 GetZRotator(Matrix3x3 rotator)
    {
        var result = new Vector3(rotator.A.Z, rotator.B.Z, rotator.C.Z);

        return result;
    }
    
    protected Vector3 CalcRotatorVector(Vector3 vector, Matrix3x3 rot)
    {
        var result = new Vector3
        {
            X = Sum(vector * GetXRotator(rot)),
            Y = Sum(vector * GetYRotator(rot)),
            Z = Sum(vector * GetZRotator(rot))
        };

        return result;
    }

    protected string Format(float value) => $"{value:0.####}";
}