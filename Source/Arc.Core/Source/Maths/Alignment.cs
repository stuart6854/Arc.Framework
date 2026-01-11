using System.Numerics;
using System.Runtime.CompilerServices;

namespace Arc.Core;

public static partial class Maths
{
    public static T AlignUp<T>(T value, T alignment) where T : IBinaryInteger<T>
    {
        var mask = alignment - T.One;
        return (value + mask) & ~mask;
    }

    public static int AlignObjectSize<T>(int alignment) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        var aligned = AlignUp(size, alignment);
        return aligned;
    }
}