namespace ApexToolsLauncher.Core.Libraries;

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
}