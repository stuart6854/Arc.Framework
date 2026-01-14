using Vortice.Direct3D11;

namespace Arc.Graphics.D3D11;

public class GfxBufferD3D11 : IGfxBuffer
{
    public ID3D11Buffer Buffer { get; }
    public BufferDesc Desc { get; }

    internal GfxBufferD3D11(ID3D11Buffer buffer, BufferDesc desc)
    {
        Buffer = buffer;
        Desc = desc;
    }
}