using System.Numerics;
using Arc.Core;
using Arc.Core.Extensions;
using Arc.Platform;
using Arc.Runtime;
using Arc.Ecs;
using Arc.Graphics;
using Arc.Graphics.D3D11;
using Arc.Samples.BouncingBoxes;
using Vortice.Mathematics;
using Rect = Arc.Graphics.Rect;
using Viewport = Arc.Graphics.Viewport;

// Setup logging
LoggerFactory.AddGlobalSink(new ConsoleSink());

// Setup runtime
Runtime.TargetFrameRate = 144;
// Start runtime with module
Runtime.Run(new SpaceShooterModule());

internal class SpaceShooterModule : IRuntimeModule
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(SpaceShooterModule));

    private Window? _window;

    private IGfxDevice? _gfxDevice;
    private IGfxSwapchain? _gfxSwapchain;

    private World _world = null!;

    private const float ViewHeight = 20.0f;
    private float ViewWidth => ViewHeight * _window!.AspectRatio;

    private float ViewHalfWidth => ViewWidth * 0.5f;
    private float ViewHalfHeight => ViewHeight * 0.5f;

    private const float EntitySize = 0.25f;
    private const float EntityHalfSize = EntitySize * 0.5f;

    public bool OnInitialize()
    {
        Platform.Initialize();
        _window = Platform.Create("Arc.Samples.SpaceShooter", 1280, 720);

        _gfxDevice = GfxDeviceD3D11.Create(true);
        _gfxSwapchain = _gfxDevice.CreateSwapchain(_window!.NativeHandle, (uint)_window!.Size.X, (uint)_window!.Size.Y);
        Renderer2D.Init(_gfxDevice);

        _world = new World();

        var rnd = new Random();
        const uint entityCount = 5_000;
        for (var i = 0; i < entityCount; i++)
        {
            var e = _world.CreateEntity();
            ref var pos = ref _world.Ensure<Position>(e);
            pos.Value = new Vector2(
                rnd.NextNormalizedSingle() * ((ViewWidth - EntitySize) / 2),
                rnd.NextNormalizedSingle() * ((ViewHeight - EntitySize) / 2)
            );

            ref var vel = ref _world.Ensure<Velocity>(e);
            vel.Value = Vector2.Normalize(new Vector2(rnd.NextNormalizedSingle(), rnd.NextNormalizedSingle()));
        }

        Logger.Info($"{nameof(SpaceShooterModule)} initialized.");
        return true;
    }

    public void OnUpdate()
    {
        Platform.PollEvents();

        if (_window is { CloseRequested: true })
            Runtime.Stop();

        _window?.Title = $"Arc.Samples.BouncingBoxes - {Time.FramesPerSecond}fps ({Time.DeltaTime * 1000.0f:N0}ms)";

        /* Simulation */

        // Movement - applies velocity to position
        _world.Query<Position, Velocity>().ForEach((ref p, ref v) => { p.Value += v.Value * Time.DeltaTime; });
        // Bounds check - if entity position goes past screen edge, velocity is "bounced"
        _world.Query<Position, Velocity>().ForEach((ref p, ref v) =>
            {
                ref var pos = ref p.Value;
                ref var vel = ref v.Value;
                // Left wall
                if (pos.X - EntityHalfSize <= -ViewHalfWidth)
                {
                    pos.X = -ViewHalfWidth + EntityHalfSize;
                    vel.X *= -1;
                }
                // Right wall
                if (pos.X + EntityHalfSize >= ViewHalfWidth)
                {
                    pos.X = ViewHalfWidth - EntityHalfSize;
                    vel.X *= -1;
                }
                // Bottom wall
                if (pos.Y - EntityHalfSize <= -ViewHalfHeight)
                {
                    pos.Y = -ViewHalfHeight + EntityHalfSize;
                    vel.Y *= -1;
                }
                // Top wall
                if (pos.Y + EntityHalfSize >= ViewHalfHeight)
                {
                    pos.Y = ViewHalfHeight - EntityHalfSize;
                    vel.Y *= -1;
                }
            }
        );

        /* Rendering */

        var cmdList = _gfxDevice!.GetImmediateCmdList();
        cmdList.Begin();

        cmdList.ClearRenderTarget(_gfxSwapchain!.CurrentTarget);
        cmdList.SetRenderTarget(_gfxSwapchain!.CurrentTarget);
        cmdList.SetViewport(new Viewport(_window!.Size.X, _window!.Size.Y));
        cmdList.SetScissorRect(new Rect(_window!.Size.X, _window!.Size.Y));

        Renderer2D.SetCamera(ViewWidth, ViewHeight);

        _world.Query<Position>().ForEach((ref p) => { Renderer2D.DrawQuad(p.Value, Vector2.One * EntitySize, Colors.IndianRed); });

        Renderer2D.Flush(cmdList);

        cmdList.End();
        _gfxSwapchain?.Present(1);
    }

    public void OnShutdown()
    {
        _world = null!;

        Renderer2D.Shutdown();
        _gfxSwapchain?.Dispose();
        _gfxSwapchain = null;
        _gfxDevice?.Dispose();
        _gfxDevice = null;

        _window = null;

        Platform.Shutdown();

        Logger.Info($"{nameof(SpaceShooterModule)} shutdown.");
    }
}