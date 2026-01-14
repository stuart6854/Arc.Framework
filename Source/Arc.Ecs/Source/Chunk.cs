namespace Arc.Ecs;

public sealed class Chunk
{
    // internal static uint ChunkByteBudget => 16u * 1024u; // 64kb

    public Archetype Archetype { get; init; }
    public Entity[] Entities { get; }
    public Array[] Columns { get; }
    public uint Count;

    public bool IsFull => Count == Entities.Length;

    public Chunk(Archetype archetype, IList<int> typeIndices, uint capacity)
    {
        Archetype = archetype;
        Entities = new Entity[capacity];
        Columns = new Array[typeIndices.Count];
        for (var i = 0; i < Columns.Length; i++)
        {
            var type = ComponentRegistry.Get(typeIndices[i]).ManagedType;
            Columns[i] = Array.CreateInstance(type, capacity);
        }
        Count = 0;
    }

    public int GetColumnTypeIndex(int columnIndex)
    {
        if (columnIndex >= Archetype.ComponentCount)
            return 0;
        return Archetype.Key.TypeIndices[columnIndex];
    }

    public int GetColumnIndex(int typeIndex) => Archetype.Key.Mask.CountBitsUpto(typeIndex) - 1;
    public int GetColumnIndex<T>() => GetColumnIndex(ComponentRegistry.Get<T>().TypeIndex);

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