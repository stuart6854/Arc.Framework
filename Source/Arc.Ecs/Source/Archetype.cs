using Arc.Core;

namespace Arc.Ecs;

public readonly struct ArchetypeKey
{
    public List<int> TypeIndices { get; } = [];
    public ComponentMask Mask { get; } = new();

    public ArchetypeKey() { }
    public ArchetypeKey(params int[] typeIndices)
    {
        TypeIndices.AddRange(typeIndices);
        TypeIndices.Sort();
        foreach (var typeIndex in TypeIndices)
            Mask.SetBit(typeIndex);
    }

    public ArchetypeKey(ArchetypeKey otherKey, int typeIndexToAdd)
    {
        TypeIndices.AddRange(otherKey.TypeIndices);
        TypeIndices.Add(typeIndexToAdd);
        TypeIndices.Sort();
        foreach (var typeIndex in TypeIndices)
            Mask.SetBit(typeIndex);
    }

    public bool Has(int typeIndex) => Mask.HasBit(typeIndex);
    public bool Has<T>() => Has(ComponentRegistry.Get<T>().TypeIndex);

    public bool Equals(ArchetypeKey other) => Mask.Equals(other.Mask);
    public override bool Equals(object? obj) => obj is ArchetypeKey other && Equals(other);
    public override int GetHashCode() => TypeIndices.GetHashCode();
    public static bool operator ==(ArchetypeKey left, ArchetypeKey right) => left.Equals(right);
    public static bool operator !=(ArchetypeKey left, ArchetypeKey right) => !left.Equals(right);
}

/// <summary>
/// An archetype is a specific set of components.
/// </summary>
public class Archetype(uint uniqueId, ArchetypeKey key)
{
    public static uint ChunkCapacity => 1024;

    public uint UniqueId { get; } = uniqueId;
    public ArchetypeKey Key { get; } = key;
    public ComponentMask Mask => Key.Mask;
    public uint ComponentCount => (uint)Key.TypeIndices.Count;

    public int EntityCount { get; set; }

    public List<Chunk> Chunks { get; } = [];

    public void AddNewEntity(Entity entity, out int chunkIndex, out int componentIndex)
    {
        componentIndex = EntityCount++;
        Chunk chunk;
        if (Chunks.Count == 0 || Chunks.Last().IsFull)
        {
            chunk = new Chunk(this, Key.TypeIndices, ChunkCapacity);
            chunkIndex = Chunks.Count;
            Chunks.Add(chunk);
        }
        else
        {
            chunk = Chunks.Last();
            chunkIndex = Chunks.Count - 1;
        }

        Assert.NotNull(chunk);
        chunk.AddEntity(entity);
    }
}