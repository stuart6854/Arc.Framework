using System.Numerics;
using Arc.Ecs;

namespace Arc.Samples.BouncingBoxes;

[Component]
public struct Position
{
    public Vector2 Value;
}