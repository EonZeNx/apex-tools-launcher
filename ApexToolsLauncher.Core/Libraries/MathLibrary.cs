using System.Globalization;
using System.Numerics;

namespace ApexToolsLauncher.Core.Libraries;

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

    public static Matrix3x3 Zero() => new(0);
    
    public override string ToString()
    {
        return $"({A}, {B}, {C})";
    }
}

public static class MathLibrary
{
    public static ulong AlignDistance(ulong value, ulong align)
    {
        if (value == 0) return 0;
        if (align == 0) return 0;
        
        var desiredAlignment = (align - (value % align)) % align;
        return desiredAlignment;
    }
    
    public static uint Align(uint value, uint align)
    {
        if (value == 0) return 0;
        if (align == 0) return value;
        
        var desiredAlignment = AlignDistance(value, align);
        return value + (uint) desiredAlignment;
    }
    
    public static byte HexToByte(string value)
    {
        return value.Length < 1
            ? (byte) 0 : Convert.ToByte(value, 16);
    }
    
    public static uint HexToUInt(string value)
    {
        if (value.Length < 1) return 0;
        
        // todo: this might break things for RTPC
        // var safeValue = "";
        // for (var i = value.Length - 2; i >= 0; i -= 2)
        // {
        //     safeValue += value[i..(i + 2)];
        // }
        
        return uint.Parse(value, NumberStyles.HexNumber);
    }
}