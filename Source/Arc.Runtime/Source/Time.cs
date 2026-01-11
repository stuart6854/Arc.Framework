using System.Diagnostics;
using Cyotek.Collections.Generic;

namespace Arc.Runtime;

public static class Time
{
    public static float TimeSinceStartup => (float)Stopwatch.GetElapsedTime(_startTimestamp, Stopwatch.GetTimestamp()).TotalSeconds;
    public static float DeltaTime { get; internal set; }
    public static uint FramesPerSecond { get; internal set; }

    private static long _startTimestamp;
    private static long _lastFrameTimestamp;

    private static float _fpsAccumTime;
    private static uint _fpsFrameCount;

    internal static void Start() => _lastFrameTimestamp = _startTimestamp = Stopwatch.GetTimestamp();
    internal static void NextFrame()
    {
        var nowTimestamp = Stopwatch.GetTimestamp();
        DeltaTime = (float)Stopwatch.GetElapsedTime(_lastFrameTimestamp, nowTimestamp).TotalSeconds;
        _lastFrameTimestamp = nowTimestamp;

        // Calculate FPS using accumulated delta
        _fpsAccumTime += DeltaTime;
        _fpsFrameCount++;
        if (_fpsAccumTime >= 0.5f)
        {
            FramesPerSecond = (uint)(_fpsFrameCount / _fpsAccumTime);
            _fpsAccumTime -= 0.5f;
            _fpsFrameCount = 0;
        }
    }
}