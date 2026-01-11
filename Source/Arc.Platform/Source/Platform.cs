using Arc.Core;
using static SDL3.SDL;

namespace Arc.Platform;

public static class Platform
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(Platform));

    private static Dictionary<uint, Window> _windows = [];

    public static void Initialize()
    {
        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            Logger.Error($"SDL3 failed to initialize: {SDL_GetError()}");
            return;
        }

        Logger.Info("SDL3 initialized.");
    }

    public static void PollEvents()
    {
        while (SDL_PollEvent(out var evt))
        {
            if (evt.type >= (uint)SDL_EventType.SDL_EVENT_WINDOW_FIRST || evt.type <= (uint)SDL_EventType.SDL_EVENT_WINDOW_LAST)
            {
                var wndId = evt.window.windowID;
                if (_windows.TryGetValue(wndId, out var wnd))
                    wnd.HandleEvent(evt.window);
            }
        }
    }

    public static void Shutdown()
    {
        SDL_Quit();

        Logger.Info("SDL3 has shut down successfully.");
    }

    public static Window? Create(string title, int width, int height)
    {
        var flags = (SDL_WindowFlags)0; // SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        var wnd = SDL_CreateWindow(title, width, height, flags);
        if (wnd == IntPtr.Zero)
        {
            Logger.Error($"Failed to create SDL3 window: {SDL_GetError()}");
            return null;
        }

        Logger.Debug($"Created SDL3 window: \"{title}\", {width}x{height}");
        var window = new Window(wnd);
        _windows.Add(SDL_GetWindowID(window.Handle), window);
        return window;
    }
}