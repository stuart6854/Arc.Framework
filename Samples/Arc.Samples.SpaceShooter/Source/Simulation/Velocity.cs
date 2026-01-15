using System.Numerics;
using Arc.Ecs;

namespace Arc.Samples.SpaceShooter;

[Component]
public struct Velocity
{
    public Vector2 Value;
}