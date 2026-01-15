using System.Numerics;
using System.Reflection;
using Arc.Core;
using Arc.Ecs;
using Arc.Graphics;
using Arc.Graphics.D3D11;
using Arc.Platform;
using Arc.Runtime;
using Arc.Samples.SpaceShooter;
using Vortice.Mathematics;
using Rect = Arc.Graphics.Rect;
using Viewport = Arc.Graphics.Viewport;

// Setup logging
LoggerFactory.AddGlobalSink(new ConsoleSink());

// Setup runtime
Runtime.TargetFrameRate = 144;
// Start runtime with module
Runtime.Run(new SpaceShooterModule());

namespace Arc.Samples.SpaceShooter
{
    internal class SpaceShooterModule : IRuntimeModule
    {
        private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(SpaceShooterModule));

        private Window? _window;

        private IGfxDevice? _gfxDevice;
        private IGfxSwapchain? _gfxSwapchain;

        private World _world = null!;

        private const float ViewHeight = 20.0f;
        private float ViewWidth => ViewHeight * _window!.AspectRatio;

        private const float PlayerSize = 1.0f;

        private Texture2D PlayerTexture = null!;

        public bool OnInitialize()
        {
            Platform.Platform.Initialize();
            _window = Platform.Platform.Create("Arc.Samples.SpaceShooter", 1280, 720);

            _gfxDevice = GfxDeviceD3D11.Create(true);
            _gfxSwapchain = _gfxDevice.CreateSwapchain(_window!.NativeHandle, (uint)_window!.Size.X, (uint)_window!.Size.Y);
            Renderer2D.Init(_gfxDevice);

            _world = new World();

            var playerEntity = _world.CreateEntity();
            _world.Add<PlayerTag>(playerEntity);
            ref var pos = ref _world.Ensure<Position>(playerEntity);

            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string resourceName = resources.First(r => r.EndsWith("ship_G.png"));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!)
            using (StreamReader reader = new StreamReader(stream))
                PlayerTexture = Texture2D.FromStream(_gfxDevice, stream);

            Logger.Info($"{nameof(SpaceShooterModule)} initialized.");
            return true;
        }

        public void OnUpdate()
        {
            Platform.Platform.PollEvents();

            if (_window is { CloseRequested: true })
                Runtime.Runtime.Stop();

            _window?.Title = $"Arc.Samples.BouncingBoxes - {Time.FramesPerSecond}fps ({Time.DeltaTime * 1000.0f:N0}ms)";

            /* Simulation */

            // Movement - applies velocity to position
            _world.Query<Position, Velocity>().ForEach((ref p, ref v) => { p.Value += v.Value * Time.DeltaTime; });

            /* Rendering */

            var cmdList = _gfxDevice!.GetImmediateCmdList();
            cmdList.Begin();

            cmdList.ClearRenderTarget(_gfxSwapchain!.CurrentTarget);
            cmdList.SetRenderTarget(_gfxSwapchain!.CurrentTarget);
            cmdList.SetViewport(new Viewport(_window!.Size.X, _window!.Size.Y));
            cmdList.SetScissorRect(new Rect(_window!.Size.X, _window!.Size.Y));

            Renderer2D.SetCamera(ViewWidth, ViewHeight);

            _world.Query<Position>().With<PlayerTag>().ForEach((ref p) => { Renderer2D.DrawQuad(p.Value, Vector2.One * PlayerSize, Colors.White, PlayerTexture); });

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

            Platform.Platform.Shutdown();

            Logger.Info($"{nameof(SpaceShooterModule)} shutdown.");
        }
    }
}