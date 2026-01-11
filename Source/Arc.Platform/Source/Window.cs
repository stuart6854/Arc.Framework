using Arc.Core;
using Vortice.Mathematics;
using static SDL3.SDL;

namespace Arc.Platform;

public class Window
{
    private static ILogger Logger { get; } = LoggerFactory.GetLogger(nameof(Window));

    internal Window(IntPtr wnd) => Handle = wnd;

    ~Window()
    {
        SDL_DestroyWindow(Handle);
        Logger.Debug($"Destroyed SDL3 window");
    }

    public IntPtr Handle { get; }
    public IntPtr NativeHandle => SDL_GetPointerProperty(SDL_GetWindowProperties(Handle), SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);
    public string Title
    {
        get => SDL_GetWindowTitle(Handle);
        set => SDL_SetWindowTitle(Handle, value);
    }
    public Int2 Size
    {
        get
        {
            SDL_GetWindowSizeInPixels(Handle, out var x, out var y);
            return new Int2(x, y);
        }
        set => SDL_SetWindowSize(Handle, value.X, value.Y);
    }
    public bool CloseRequested { get; private set; }

    internal void HandleEvent(SDL_WindowEvent evt)
    {
        if (evt.windowID != SDL_GetWindowID(Handle))
            return;
        if (evt.type < SDL_EventType.SDL_EVENT_WINDOW_FIRST || evt.type > SDL_EventType.SDL_EVENT_WINDOW_LAST)
            return;

        switch (evt.type)
        {
            case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                CloseRequested = true;
                break;
        }
    }
}