using Vortice.Direct3D11;

namespace Arc.Graphics.D3D11;

public sealed class GfxTextureD3D11 : IGfxTexture
{
    public ID3D11Resource Resource { get; private init; }
    public TextureDesc Desc { get; private init; }

    internal GfxTextureD3D11(ID3D11Resource resource, TextureDesc desc)
    {
        Resource = resource;
        Desc = desc;
    }
}

public sealed class GfxTextureViewD3D11 : IGfxTextureView
{
    public ID3D11View View { get; private init; }
    public TextureViewDesc Desc { get; }

    internal GfxTextureViewD3D11(ID3D11View view, TextureViewDesc desc)
    {
        View = view;
        Desc = desc;
    }
}