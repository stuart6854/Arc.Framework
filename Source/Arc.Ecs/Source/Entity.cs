namespace Arc.Ecs;

public readonly struct Entity : IEquatable<Entity>
{
    public static Entity Null { get; } = new(0, 0);

    public uint Id { get; } = 0;
    public uint Version { get; } = 0;
    public ulong Packed => ((ulong)Id << 32) | Version;

    public Entity(uint id, uint version)
    {
        Id = id;
        Version = version;
    }
    public Entity(ulong packed)
    {
        Id = (uint)(packed >> 32);
        Version = (uint)(packed & 0xFFFFFFFF);
    }

    public override string ToString() => $"Entity({Id}, {Version})";

    public bool Equals(Entity other) => Packed == other.Packed;
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Entity)obj);
    }
    public override int GetHashCode() => Packed.GetHashCode();
    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}