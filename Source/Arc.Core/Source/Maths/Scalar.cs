using System.Numerics;

namespace Arc.Core;

public static partial class Maths
{
    public static T Clamp<T>(T value, T min, T max) where T : INumber<T> => value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

    public static T Lerp<T>(T a, T b, T t) where T : INumber<T> => a + (b - a) * t;
}