using Arc.Core;

namespace Arc.Ecs;

public record struct ComponentTypeInfo(ulong Id, Type ManagedType, uint Size, bool ContainsRefs, bool IsTag);

public static partial class ComponentRegistry
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(ComponentRegistry));

    public static List<ComponentTypeInfo> _components = [];

    public static IReadOnlyList<ComponentTypeInfo> Components => _components;

    public static void Register(ComponentTypeInfo typeInfo) { _components.Add(typeInfo); }

    public static ComponentTypeInfo? Get(ulong id) => _components.Find(c => c.Id == id);
}

public static class ComponentInfo<T>
{
    public static Type ManagedType => typeof(T);
    public static ulong Id => FNV1A.ComputeHash($"{ManagedType.Namespace}{(string.IsNullOrEmpty(ManagedType.Namespace) ? "" : ".")}{ManagedType.Name}");
}