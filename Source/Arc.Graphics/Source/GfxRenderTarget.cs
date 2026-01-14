namespace Arc.Graphics;

public struct RenderTargetDesc
{
    public uint Width;
    public uint Height;
    public uint SampleCount = 1;
    public Format[] ColorFormats = [];
    public Format DepthFormat = Format.Unknown;

    public RenderTargetDesc() { }
}

public sealed class GfxRenderTarget : IDisposable
{
    public IGfxTexture[] ColorTextures { get; set; } = [];
    public IGfxTexture? DepthTexture { get; set; } = null!;
    
    public IGfxTextureView[] ColorRTVs { get; set; } = [];
    public IGfxTextureView? DepthDSV { get; set; } = null!;

    public void Dispose()
    {
        ColorTextures = [];
        DepthTexture = null;
    }
}