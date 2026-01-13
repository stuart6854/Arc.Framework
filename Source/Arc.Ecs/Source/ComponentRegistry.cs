using Arc.Core;

namespace Arc.Ecs;

/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="ManagedType"></param>
/// <param name="Size"></param>
/// <param name="ContainsRefs"></param>
/// <param name="IsTag"></param>
/// <param name="BitIndex">Bit position for filters/archetypes. Set automatically.</param>
public record struct ComponentTypeInfo(ulong Id, Type ManagedType, uint Size, bool ContainsRefs, bool IsTag, int BitIndex);

public static partial class ComponentRegistry
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(ComponentRegistry));

    public static List<ComponentTypeInfo> _components = [];

    public static IReadOnlyList<ComponentTypeInfo> Components => _components;

    public static void Register(ComponentTypeInfo typeInfo)
    {
        typeInfo.BitIndex = _components.Count;
        _components.Add(typeInfo);
    }

    public static ComponentTypeInfo? Get(ulong id) => _components.Find(c => c.Id == id);
}

public static class ComponentInfo<T>
{
    public static Type ManagedType => typeof(T);
    public static ulong Id => FNV1A.ComputeHash($"{ManagedType.Namespace}{(string.IsNullOrEmpty(ManagedType.Namespace) ? "" : ".")}{ManagedType.Name}");
}