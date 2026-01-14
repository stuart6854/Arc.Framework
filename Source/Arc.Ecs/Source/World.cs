using System.Runtime.InteropServices;
using Arc.Core;

namespace Arc.Ecs;

public class World
{
    private readonly List<EntityRecord> _entities = [];
    private readonly Queue<uint> _freeEntityIds = [];
    private uint _nextEntityId = 1;

    private Dictionary<ArchetypeKey, Archetype> _archetypes = [];
    private uint _nextArchetypeId = 1;

    internal ICollection<Archetype> Archetypes => _archetypes.Values;

    #region Entities

    public Entity CreateEntity()
    {
        uint id;
        if (_freeEntityIds.Count > 0)
            id = _freeEntityIds.Dequeue();
        else
        {
            id = _nextEntityId++;
            _entities.Add(new EntityRecord());
        }

        ref var entityRecord = ref GetEntityRecord(id);
        var version = entityRecord.Version;
        var e = new Entity(id, version);

        var archetype = GetOrCreateArchetype(new ArchetypeKey());
        archetype.AddNewEntity(e, out var chunkIndex, out var compIndex);
        entityRecord.Archetype = archetype;
        entityRecord.ChunkIndex = chunkIndex;
        entityRecord.Row = compIndex;

        Assert.NotNull(entityRecord.Archetype);
        Assert.IsTrue(entityRecord.ChunkIndex >= 0);
        Assert.IsTrue(entityRecord.Row >= 0);

        return e;
    }

    public void DestroyEntity(Entity entity)
    {
        if (entity == Entity.Null)
            return; // Silently fail

        ref var entityRecord = ref GetEntityRecord(entity.Id);
        if (entityRecord.Version != entity.Version)
            return; // Silently fail

        entityRecord.Version++;

        _freeEntityIds.Enqueue(entity.Id);
    }

    #endregion

    #region Components

    public bool Has<T>(Entity entity)
    {
        if (entity == Entity.Null)
            return false; // Silently fail

        ref var entityRecord = ref GetEntityRecord(entity.Id);
        if (entityRecord.Version != entity.Version)
            return false; // Silently fail

        if (entityRecord.Archetype == null)
            return false;

        return entityRecord.Archetype.Key.Has<T>();
    }

    public void Add<T>(Entity entity)
    {
        if (entity == Entity.Null)
            return; // Silently fail

        ref var entityRecord = ref GetEntityRecord(entity.Id);
        if (entityRecord.Version != entity.Version)
            return; // Silently fail

        var typeId = ComponentInfo<T>.Id;

        var srcArchetype = entityRecord.Archetype!;
        var key = new ArchetypeKey(srcArchetype.Key, typeId);

        var dstArchetype = GetOrCreateArchetype(key);
        dstArchetype.AddNewEntity(entity, out var dstChunkIndex, out var dstRow);
        MoveComponentsToChunk(srcArchetype.Chunks[entityRecord.ChunkIndex], entityRecord.Row, dstArchetype.Chunks[dstChunkIndex], dstRow);

        entityRecord.Archetype = dstArchetype;
        entityRecord.ChunkIndex = dstChunkIndex;
        entityRecord.Row = dstRow;
    }

    public void Remove<T>(Entity entity)
    {
        if (entity == Entity.Null)
            return; // Silently fail

        ref var entityRecord = ref GetEntityRecord(entity.Id);
        if (entityRecord.Version != entity.Version)
            return; // Silently fail

        var typeId = ComponentInfo<T>.Id;

        var srcArchetype = entityRecord.Archetype!;
        var key = new ArchetypeKey(srcArchetype.Key.ComponentTypeIds.Where(t => t != typeId).ToArray());

        var dstArchetype = GetOrCreateArchetype(key);
        dstArchetype.AddNewEntity(entity, out var dstChunkIndex, out var dstCompIndex);
        MoveComponentsToChunk(srcArchetype.Chunks[entityRecord.ChunkIndex], entityRecord.Row, dstArchetype.Chunks[dstChunkIndex], dstCompIndex);

        entityRecord.Archetype = dstArchetype;
        entityRecord.ChunkIndex = dstChunkIndex;
        entityRecord.Row = dstCompIndex;
    }

    public ref readonly T Get<T>(Entity entity) { return ref GetMutable<T>(entity); }

    public ref T GetMutable<T>(Entity entity)
    {
        Assert.IsTrue(entity != Entity.Null);

        ref var entityRecord = ref GetEntityRecord(entity.Id);
        Assert.IsTrue(entityRecord.Version == entity.Version);
        Assert.NotNull(entityRecord.Archetype);

        var chunk = entityRecord.Archetype.Chunks[entityRecord.ChunkIndex];
        var columnIndex = chunk.GetColumnIndex<T>();
        var column = (T[])chunk.Columns[columnIndex];
        return ref column[entityRecord.Row];
    }

    public ref T Ensure<T>(Entity entity)
    {
        if (!Has<T>(entity))
            Add<T>(entity);
        return ref GetMutable<T>(entity);
    }

    #endregion

    #region Queries

    public Query Query() => new(this);
    public Query<T0> Query<T0>() => new(this);

    #endregion

    #region Internal

    private ref EntityRecord GetEntityRecord(uint id) { return ref CollectionsMarshal.AsSpan(_entities)[(int)id - 1]; }

    private Archetype GetOrCreateArchetype(ArchetypeKey key)
    {
        if (!_archetypes.TryGetValue(key, out var archetype))
        {
            archetype = new Archetype(_nextArchetypeId++, key);
            _archetypes.Add(key, archetype);
        }
        return archetype;
    }

    /// <summary>
    /// Move an entities components from srcChunk[srcRow] to dstChunk[dstRow].
    /// Constructing, copying or destroying where required.
    /// </summary>
    /// <param name="srcChunk">The source Chunk</param>
    /// <param name="srcRow">The index of the source row in srcChunk</param>
    /// <param name="dstChunk">The destination Chunk</param>
    /// <param name="dstRow">The index of the destination row in dstChunk</param>
    private void MoveComponentsToChunk(Chunk? srcChunk, int srcRow, Chunk dstChunk, int dstRow)
    {
        bool isMovingLastRow = srcChunk != null && srcRow == srcChunk.Count - 1;

        var srcColumnCount = srcChunk?.Archetype.ComponentCount ?? 0;
        var dstColumnCount = dstChunk.Archetype.ComponentCount;

        int srcColumnIndex = 0;
        int dstColumnIndex = 0;

        // 1) Merge common, removed, added components
        while (srcColumnIndex < srcColumnCount || dstColumnIndex < dstColumnCount)
        {
            // Only dst types remain -> components added
            if (srcColumnIndex >= srcColumnCount)
            {
                var dstTypeId = dstChunk.GetComponentId(dstRow);
                var dstType = ComponentRegistry.Get(dstTypeId)!.Value.ManagedType;
                var dstColumn = dstChunk.Columns[dstColumnIndex];
                dstColumn.SetValue(Activator.CreateInstance(dstType), dstRow);

                dstColumnIndex++;
                continue;
            }

            // Only src types remain -> components removed
            if (dstColumnIndex >= dstColumnCount)
            {
                if (!isMovingLastRow)
                {
                    var srcTypeId = srcChunk!.GetComponentId(srcRow);
                    var srcType = ComponentRegistry.Get(srcTypeId)!.Value.ManagedType;
                    var srcColumn = srcChunk.Columns[srcColumnIndex];
                    srcColumn.SetValue(null /*Activator.CreateInstance(srcType)*/, srcRow);
                }

                srcColumnIndex++;
                continue;
            }

            {
                var srcTypeId = srcChunk!.GetComponentId(srcColumnIndex);
                var dstTypeId = dstChunk.GetComponentId(dstColumnIndex);

                if (srcTypeId == dstTypeId)
                {
                    // Type present in both chunks -> move value from src -> dst
                    var srcColumn = srcChunk.Columns[srcColumnIndex];
                    var dstColumn = dstChunk.Columns[dstColumnIndex];

                    dstColumn.SetValue(srcColumn.GetValue(srcRow), dstRow);

                    srcColumnIndex++;
                    dstColumnIndex++;
                }
                else if (srcTypeId < dstTypeId)
                {
                    // Type removed
                    var srcType = ComponentRegistry.Get(srcTypeId)!.Value.ManagedType;
                    var srcColumn = srcChunk.Columns[srcColumnIndex];
                    srcColumn.SetValue(null /*Activator.CreateInstance(srcType)*/, srcRow);

                    srcColumnIndex++;
                }
                else // srcTypeId > dstTypeId
                {
                    // Type added
                    var dstType = ComponentRegistry.Get(dstTypeId)!.Value.ManagedType;
                    var dstColumn = dstChunk.Columns[dstColumnIndex];
                    dstColumn.SetValue(Activator.CreateInstance(dstType), dstRow);

                    dstColumnIndex++;
                }
            }
        }

        // Swap remove entity row froms src chunk (keep packed)
        if (srcChunk != null)
        {
            srcChunk.RemoveEntity((uint)srcRow, out var swappedEntity);
            if (swappedEntity != Entity.Null)
            {
                ref var swappedEntityRecord = ref GetEntityRecord(swappedEntity.Id);
                swappedEntityRecord.ChunkIndex = srcRow;
            }
        }
    }

    #endregion
}

internal struct EntityRecord()
{
    public uint Version = 0;
    public Archetype? Archetype = null; // Currently assigned archetype
    public int ChunkIndex = -1;         // Chunk index in archetype
    public int Row = -1;                // Component index in chunk
}