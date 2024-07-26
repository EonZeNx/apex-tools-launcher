namespace ATL.Core.Libraries;

public static class MathLibrary
{
    public static uint Align(uint value, uint align)
    {
        if (value == 0) return 0;
        if (align == 0) return value;
        
        var desiredAlignment = (align - (value % align)) % align;
        return value + desiredAlignment;
    }
}