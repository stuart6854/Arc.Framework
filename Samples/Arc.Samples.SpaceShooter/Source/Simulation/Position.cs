using System.Numerics;
using Arc.Ecs;

namespace Arc.Samples.SpaceShooter;

[Component]
public struct Position
{
    public Vector2 Value;
}