using Arc.Core;
using Arc.Platform;
using Arc.Runtime;
using Arc.Ecs;

// Setup logging
LoggerFactory.AddGlobalSink(new ConsoleSink());

// Setup runtime
Runtime.TargetFrameRate = 144;
// Start runtime with module
Runtime.Run(new BridgeheadModule());

internal class BridgeheadModule : IRuntimeModule
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(BridgeheadModule));

    private Window? _window;
    private World _world = null!;

    public bool OnInitialize()
    {
        Platform.Initialize();
        _window = Platform.Create("Arc Bridgehead", 1280, 720);

        _world = new World();
        
        var e = _world.CreateEntity();
        Assert.IsFalse(_world.Has<Position>(e));
        _world.Add<Position>(e);
        Assert.IsTrue(_world.Has<Position>(e));
        ref var p0 = ref _world.GetMutable<Position>(e);
        p0.X = 123.4f;
        p0.Y = 567.8f;
        p0.Z = 901.2f;

        ref readonly var p1 = ref _world.Get<Position>(e);

        var query = _world.Query();
        var first = query.FindFirst();
        
        query.ForEach(entity => Logger.Info(entity.ToString()));
        _world.Query<Position>().ForEach((ref p) => Logger.Info($"{p.X}, {p.Y}, {p.Z}"));

        _world.Remove<Position>(e);
        Assert.IsFalse(_world.Has<Position>(e));
        _world.DestroyEntity(e);

        Logger.Info($"{nameof(BridgeheadModule)} initialized.");
        return true;
    }

    public void OnUpdate()
    {
        Platform.PollEvents();

        if (_window is { CloseRequested: true })
            Runtime.Stop();

        _window?.Title = $"Bridgehead - {Time.FramesPerSecond}fps ({Time.DeltaTime * 1000.0f:N0}ms)";
    }

    public void OnShutdown()
    {
        _window = null;
        Platform.Shutdown();

        Logger.Info($"{nameof(BridgeheadModule)} shutdown.");
    }
}

[Component]
struct Position
{
    public float X;
    public float Y;
    public float Z;

    public Position()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }
}

[Component]
struct EnemyTag { }