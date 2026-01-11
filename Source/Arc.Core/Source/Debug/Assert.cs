using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Arc.Core;

public static class Assert
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger("Assert");

    public static void IsTrue([DoesNotReturnIf(false)] bool condition, string? msg = null)
    {
        if (!condition)
            Fail(msg ?? "Condition is false.");
    }

    public static void IsFalse([DoesNotReturnIf(true)] bool condition, string? msg = null)
    {
        if (condition)
            Fail(msg ?? "Condition is true.");
    }

    public static void NotNull<T>([NotNull] T? obj, string? msg = null) where T : class
    {
        if (obj is null)
            Fail(msg ?? $"{nameof(obj)} is null.");
    }

    public static void NotNull([NotNull] object? obj, string? msg = null)
    {
        if (obj is null)
            Fail(msg ?? $"{nameof(obj)} is null.");
    }

    [DoesNotReturn]
    public static void Fail(string msg)
    {
        Logger.Fatal(msg);

        if (Debugger.IsAttached)
            Debugger.Break();

        throw new InvalidOperationException(msg);
    }
}