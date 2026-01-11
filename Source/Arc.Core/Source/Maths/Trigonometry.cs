using System.Numerics;

namespace Arc.Core;

public static partial class Maths
{
    public static float ToRadians(float degrees) => degrees * float.Pi / 180.0f;
    public static double ToRadians(double degrees) => degrees * double.Pi / 180.0;

    public static float ToDegrees(float radians) => radians * 180.0f / float.Pi;
    public static double ToDegrees(double radians) => radians * 180.0 / double.Pi;
}