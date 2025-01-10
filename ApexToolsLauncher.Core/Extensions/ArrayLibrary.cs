using System.Runtime.CompilerServices;

namespace ApexToolsLauncher.Core.Extensions;

public static class ArrayLibrary
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Empty<T>(this T[] a)
    {
        return a.Length == 0;
    }
}