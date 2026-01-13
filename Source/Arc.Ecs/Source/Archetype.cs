using Arc.Core;

namespace Arc.Ecs;

using ComponentId = ulong;
using ArchetypeKeyHash = ulong;

public readonly struct ArchetypeKey
{
    public List<ComponentId> ComponentTypeIds { get; } = [];
    public ComponentMask Mask { get; } = new();
    public ArchetypeKeyHash Hash { get; }

    public ArchetypeKey() { }
    public ArchetypeKey(params ComponentId[] componentTypeIds)
    {
        ComponentTypeIds.AddRange(componentTypeIds);
        ComponentTypeIds.Sort();
        Mask = ComponentMask.FromMax(ComponentTypeIds.Count > 0 ? ComponentRegistry.Get(ComponentTypeIds.Last())!.Value.BitIndex + 1 : 0);
        Hash = FNV1A.ComputeHash(ComponentTypeIds);
    }
    // public ArchetypeKey(params Type[] componentTypes) : this(componentTypes.Select(t => FNV1A.ComputeHash(t.FullName!)).ToArray()) { }

    public ArchetypeKey(ArchetypeKey otherKey, ComponentId idToAdd)
    {
        ComponentTypeIds.AddRange(otherKey.ComponentTypeIds);
        ComponentTypeIds.Add(idToAdd);
        ComponentTypeIds.Sort();
        Mask = ComponentMask.FromMax(ComponentTypeIds.Count > 0 ? ComponentRegistry.Get(ComponentTypeIds.Last())!.Value.BitIndex + 1 : 0);
        Hash = FNV1A.ComputeHash(ComponentTypeIds);
    }

    public bool Has(ComponentId typeId) => ComponentTypeIds.BinarySearch(typeId) >= 0;
    public bool Has<T>() => Has(ComponentInfo<T>.Id);

    public bool Equals(ArchetypeKey other) => Hash.Equals(other.Hash);
    public override bool Equals(object? obj) => obj is ArchetypeKey other && Equals(other);
    public override int GetHashCode() => ComponentTypeIds.GetHashCode();
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
    public uint ComponentCount => (uint)Key.ComponentTypeIds.Count;

    public int EntityCount { get; set; }
    // public List<Entity> Entities { get; } = [];

    public List<Chunk> Chunks { get; } = [];

    public void AddNewEntity(Entity entity, out int chunkIndex, out int componentIndex)
    {
        componentIndex = EntityCount++;
        Chunk chunk;
        if (Chunks.Count == 0 || Chunks.Last().IsFull)
        {
            chunk = new Chunk(this, Key.ComponentTypeIds, ChunkCapacity);
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