namespace Arc.Core;

/// <summary>
/// Represents an immutable universally unique identifier (UUID).
/// A UUID represents a 64-bit value;
/// </summary>
public readonly struct Uuid : IEquatable<Uuid>
{
    public static Uuid Empty { get; } = new(0);
    public static Uuid Create()
    {
        var buffer = new byte[8];
        Random.Shared.NextBytes(buffer);
        var value = BitConverter.ToUInt64(buffer);
        return new Uuid(value);
    }

    public ulong Value { get; }

    public Uuid() => Value = 0;
    public Uuid(ulong value) => Value = value;

    public bool Equals(Uuid other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Uuid other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Uuid left, Uuid right) => left.Equals(right);
    public static bool operator !=(Uuid left, Uuid right) => !left.Equals(right);

    public override string ToString() => Value.ToString();
}