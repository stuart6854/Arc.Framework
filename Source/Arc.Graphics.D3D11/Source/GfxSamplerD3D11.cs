using Vortice.Direct3D11;

namespace Arc.Graphics.D3D11;

public sealed class GfxSamplerD3D11 : IGfxSampler
{
    public ID3D11SamplerState SamplerState { get; private init; }

    internal GfxSamplerD3D11(ID3D11SamplerState samplerState) { SamplerState = samplerState; }
}