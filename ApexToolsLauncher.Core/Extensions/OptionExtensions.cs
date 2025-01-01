using System.Runtime.CompilerServices;
using RustyOptions;
using static System.ArgumentNullException;

namespace ApexToolsLauncher.Core.Extensions;

public static class OptionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> MatchSome<T>(this Option<T> self, Action<T> onSome)
        where T : notnull
    {
        ThrowIfNull(onSome);
        if (self.IsSome(out var value))
            onSome(value);

        return self;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> MatchNone<T>(this Option<T> self, Action onNone)
        where T : notnull
    {
        ThrowIfNull(onNone);
        if (self.IsNone)
            onNone();

        return self;
    }
}