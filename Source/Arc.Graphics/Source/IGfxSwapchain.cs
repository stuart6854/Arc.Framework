namespace Arc.Graphics;

public interface IGfxSwapchain : IDisposable
{
    uint Width { get; }
    uint Height { get; }

    GfxRenderTarget CurrentTarget { get; }

    void Resize(uint width, uint height);
    void Present(uint syncInterval);
}