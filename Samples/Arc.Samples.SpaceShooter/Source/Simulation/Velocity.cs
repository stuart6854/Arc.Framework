using System.Numerics;
using Arc.Ecs;

namespace Arc.Samples.BouncingBoxes;

[Component]
public struct Velocity
{
    public Vector2 Value;
}