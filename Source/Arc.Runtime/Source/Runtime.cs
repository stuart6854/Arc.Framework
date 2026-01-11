using System.Diagnostics;
using Arc.Core;

namespace Arc.Runtime;

public interface IRuntimeModule
{
    public bool OnInitialize();
    public void OnUpdate();
    public void OnShutdown();
}

public static class Runtime
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(Runtime));

    /// <summary>
    /// Sets the target frames per second (fps).
    /// Set to 0 to disable.
    /// Set to 60 to target 60 fps.
    /// </summary>
    public static int TargetFrameRate { get; set; }

    private static bool _isRunning;

    public static void Run(IRuntimeModule module)
    {
        var nextFrameTick = Stopwatch.GetTimestamp();
        var tickFrequency = Stopwatch.Frequency;

        Logger.Info("Runtime initializing...");
        Time.Start();
        _isRunning = false;
        if (module.OnInitialize())
            _isRunning = true;

        if (!_isRunning)
        {
            Logger.Error("Runtime failed to initialize. Shutting down.");
            module.OnShutdown();
            return;
        }

        Logger.Info("Runtime initialized. Starting main loop...");
        while (_isRunning)
        {
            var ticksPerFrame = tickFrequency / TargetFrameRate;
            nextFrameTick += ticksPerFrame;

            Time.NextFrame();

            module.OnUpdate();

            PrecisionWait(nextFrameTick);
        }

        Logger.Info("Runtime shutting down...");
        module.OnShutdown();
        Logger.Info("Runtime stopped.");
        Logger.Info($"Execution Time: {Time.TimeSinceStartup}");
    }

    public static void Stop()
    {
        _isRunning = false;
        Logger.Info("Runtime stop requested.");
    }

    internal static void PrecisionWait(long targetTick)
    {
        while (Stopwatch.GetTimestamp() < targetTick)
        {
            var remainingTicks = targetTick - Stopwatch.GetTimestamp();
            var remainingMs = (remainingTicks * 1000.0f) / Stopwatch.Frequency;

            if (remainingMs > 20.0f)
                Thread.Sleep(1); // Sleep while we have plenty of time left
            else
                SpinWait.SpinUntil(() => Stopwatch.GetTimestamp() >= targetTick);
        }
    }
}