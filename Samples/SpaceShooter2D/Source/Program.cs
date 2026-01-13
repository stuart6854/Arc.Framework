using Arc.Core;
using Arc.Platform;
using Arc.Runtime;
using Arc.Ecs;

// Setup logging
LoggerFactory.AddGlobalSink(new ConsoleSink());

// Setup runtime
Runtime.TargetFrameRate = 144;
// Start runtime with module
Runtime.Run(new SpaceShooter2DModule());

internal class SpaceShooter2DModule : IRuntimeModule
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(SpaceShooter2DModule));

    private Window? _window;
    private World _world = null!;

    public bool OnInitialize()
    {
        Platform.Initialize();
        _window = Platform.Create("Arc Sample - SpaceShooter2D", 1280, 720);

        _world = new World();

        Logger.Info($"{nameof(SpaceShooter2DModule)} initialized.");
        return true;
    }

    public void OnUpdate()
    {
        Platform.PollEvents();

        if (_window is { CloseRequested: true })
            Runtime.Stop();

        _window?.Title = $"Arc Sample - SpaceShooter2D - {Time.FramesPerSecond}fps ({Time.DeltaTime * 1000.0f:N0}ms)";
    }

    public void OnShutdown()
    {
        _window = null;
        Platform.Shutdown();

        Logger.Info($"{nameof(SpaceShooter2DModule)} shutdown.");
    }
}