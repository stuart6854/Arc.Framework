using Arc.Core;
using Arc.Platform;
using Arc.Runtime;

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

    public bool OnInitialize()
    {
        Platform.Initialize();
        _window = Platform.Create("Arc Bridgehead", 1280, 720);

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