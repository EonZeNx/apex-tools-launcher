namespace ApexToolsLauncher.Core.Extensions;

public static class MathExtensions
{
    public static bool AlmostEqual(this float a, float b, float delta = 0.00001f)
    {
        return Math.Abs(a - b) < delta;
    }
}