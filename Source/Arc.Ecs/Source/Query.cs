namespace Arc.Ecs;

// 
public delegate void ForEach();
public delegate void ForEach<T0>(ref T0 t0);

// With entities
public delegate void ForEachEntity(Entity e);
public delegate void ForEachEntity<T0>(Entity e, ref T0 t0);

internal struct QueryFilter()
{
    public ComponentMask All = new();
    public ComponentMask Any = new();
    public ComponentMask None = new();

    public readonly bool Matches(in ComponentMask mask)
    {
        if (!mask.ContainsAll(All))
            return false;
        if (!Any.IsEmpty && !mask.ContainsAny(Any))
            return false;
        if (!mask.ContainsNone(None))
            return false;
        return true;
    }
}

public class QueryBase
{
    protected World World;
    internal QueryFilter Filter = new();

    public QueryBase(World world)
    {
        World = world;
        Filter.None.SetBit(ComponentRegistry.Get<Disabled>().TypeIndex);
    }

    public Entity FindFirst()
    {
        var matchingArchetypes = GetMatchingArchetypes();
        foreach (var archetype in matchingArchetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var entityCount = chunk.Count;
                if (entityCount == 0)
                    continue;

                return chunk.Entities[0];
            }
        }
        return Entity.Null;
    }

    protected Archetype[] GetMatchingArchetypes()
    {
        List<Archetype> matches = [];
        var allArchetypes = World.Archetypes;
        foreach (var archetype in allArchetypes)
        {
            if (ArchetypeMatches(archetype, Filter))
                matches.Add(archetype);
        }
        return matches.ToArray();
    }

    internal static bool ArchetypeMatches(Archetype archetype, QueryFilter filter)
    {
        // All
        if (!filter.All.IsEmpty && !archetype.Mask.ContainsAll(filter.All))
            return false;

        // Any
        if (!filter.Any.IsEmpty && !archetype.Mask.ContainsAny(filter.Any))
            return false;

        // None
        if (!filter.None.IsEmpty && !archetype.Mask.ContainsNone(filter.None))
            return false;

        return true;
    }
}

public abstract class QueryBase<TSelf> : QueryBase
    where TSelf : QueryBase<TSelf>
{
    protected QueryBase(World world) : base(world) { }

    public TSelf With<TTy0>()
    {
        Filter.All.SetBit(ComponentRegistry.Get<TTy0>().TypeIndex);
        return (TSelf)this;
    }

    public TSelf Optional<TTy0>()
    {
        Filter.Any.SetBit(ComponentRegistry.Get<TTy0>().TypeIndex);
        return (TSelf)this;
    }

    public TSelf Without<TTy0>()
    {
        Filter.None.SetBit(ComponentRegistry.Get<TTy0>().TypeIndex);
        return (TSelf)this;
    }

    public TSelf WithDisabled()
    {
        Filter.None.ClearBit(ComponentRegistry.Get<Disabled>().TypeIndex);
        return (TSelf)this;
    }
}

public class Query : QueryBase<Query>
{
    public Query(World world) : base(world) { }

    public void ForEach(ForEach forEach)
    {
        var matchingArchetypes = GetMatchingArchetypes();
        foreach (var archetype in matchingArchetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var entityCount = chunk.Count;
                if (entityCount == 0)
                    continue;

                for (var i = 0; i < entityCount; ++i)
                    forEach();
            }
        }
    }

    public void ForEach(ForEachEntity forEach)
    {
        var matchingArchetypes = GetMatchingArchetypes();
        foreach (var archetype in matchingArchetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var entityCount = chunk.Count;
                if (entityCount == 0)
                    continue;

                for (var i = 0; i < entityCount; ++i)
                    forEach(chunk.Entities[i]);
            }
        }
    }
}

public class Query<T0> : QueryBase<Query<T0>>
{
    public Query(World world) : base(world) { With<T0>(); }

    public void ForEach(ForEach<T0> forEach)
    {
        var matchingArchetypes = GetMatchingArchetypes();
        foreach (var archetype in matchingArchetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var entityCount = chunk.Count;
                if (entityCount == 0)
                    continue;

                var column = chunk.GetColumnIndex<T0>();
                var columnData = (T0[])chunk.Columns[column];

                for (var i = 0; i < entityCount; ++i)
                    forEach(ref columnData[i]);
            }
        }
    }

    public void ForEach(ForEachEntity<T0> forEach)
    {
        var matchingArchetypes = GetMatchingArchetypes();
        foreach (var archetype in matchingArchetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var entityCount = chunk.Count;
                if (entityCount == 0)
                    continue;

                var column = chunk.GetColumnIndex<T0>();
                var columnData = (T0[])chunk.Columns[column];

                for (var i = 0; i < entityCount; ++i)
                    forEach(chunk.Entities[i], ref columnData[i]);
            }
        }
    }
}