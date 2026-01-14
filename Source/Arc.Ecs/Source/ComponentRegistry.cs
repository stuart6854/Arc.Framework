using Arc.Core;

namespace Arc.Ecs;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ComponentAttribute : Attribute
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="ManagedType"></param>
/// <param name="Size"></param>
/// <param name="ContainsRefs"></param>
/// <param name="IsTag"></param>
/// <param name="TypeIndex">Bit position for filters/archetypes. Set automatically.</param>
public record struct ComponentTypeInfo(int TypeIndex, Type ManagedType, uint Size, bool ContainsRefs, bool IsTag);

public static partial class ComponentRegistry
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(ComponentRegistry));

    private static readonly List<ComponentTypeInfo> _components = [];
    private static readonly Dictionary<Type, ComponentTypeInfo> _typeInfos = new();

    public static IReadOnlyList<ComponentTypeInfo> Components => _components;

    public static void Register(ComponentTypeInfo typeInfo)
    {
        typeInfo.TypeIndex = _components.Count;
        _components.Add(typeInfo);
        _typeInfos[typeInfo.ManagedType] = typeInfo;
    }

    public static ComponentTypeInfo Get(int index) => _components[index];

    public static ComponentTypeInfo Get(Type type) => _typeInfos[type];
    public static ComponentTypeInfo Get<T>() => Get(typeof(T));
}