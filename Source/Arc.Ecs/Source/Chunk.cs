namespace Arc.Ecs;

internal sealed class Chunk
{
    internal static uint ChunkByteBudget => 16u * 1024u; // 64kb

    public Archetype Archetype { get; init; }
    public Entity[] Entities { get; }
    public Array[] Columns { get; }
    public uint Count;

    public bool IsFull => Count == Entities.Length;

    public Chunk(Archetype archetype, IList<ulong> typesIds, uint capacity)
    {
        Archetype = archetype;
        Entities = new Entity[capacity];
        Columns = new Array[typesIds.Count];
        for (var i = 0; i < Columns.Length; i++)
        {
            var type = ComponentRegistry.Get(typesIds[i])!.Value.ManagedType;
            Columns[i] = Array.CreateInstance(type, capacity);
        }
        Count = 0;
    }

    public ulong GetComponentId(int index)
    {
        if (index >= Archetype.ComponentCount)
            return 0;
        return Archetype.Key.ComponentTypeIds[index];
    }

    public int GetColumnIndex(ulong componentId) { return Archetype.Key.ComponentTypeIds.BinarySearch(componentId); }

    public int GetColumnIndex<T>() { return GetColumnIndex(ComponentInfo<T>.Id); }

    public uint AddEntity(Entity entity)
    {
        var index = Count++;
        Entities[index] = entity;
        return index;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="replaceRow">If >= Entity.Null, indicates what entity filled the space</param>
    public void RemoveEntity(uint row, out Entity swappedEntity)
    {
        // Pack rows - move lastRow into row
        var lastRow = Count - 1;
        if (row != lastRow)
        {
            // Move each column: last -> row
            foreach (var column in Columns)
            {
                column.SetValue(column.GetValue(lastRow), row);
                column.SetValue(null, lastRow);
            }

            swappedEntity = Entities[lastRow];

            // Update entity for swapped entity
            Entities[row] = Entities[lastRow];
            Entities[lastRow] = Entity.Null;
        }

        Count--;
        swappedEntity = Entity.Null;
    }
}