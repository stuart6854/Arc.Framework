using Vortice.Direct3D11;

namespace Arc.Graphics.D3D11;

public class GfxPipelineD3D11 : IGfxPipeline
{
    public ID3D11InputLayout? inputLayout { get; private init; }
    public ID3D11VertexShader? vs { get; private init; }
    public ID3D11PixelShader? ps { get; private init; }

    public Vortice.Direct3D.PrimitiveTopology PrimitiveTopology { get; private init; }

    public ID3D11BlendState? BlendState { get; private init; }
    public ID3D11RasterizerState? RasterState { get; private init; }
    public ID3D11DepthStencilState? DepthState { get; private init; }

    internal GfxPipelineD3D11(ID3D11InputLayout inputLayout, ID3D11VertexShader? vs, ID3D11PixelShader? ps,
        Vortice.Direct3D.PrimitiveTopology primitiveTopology, ID3D11BlendState blendState, ID3D11RasterizerState rasterState,
        ID3D11DepthStencilState depthState)
    {
        this.inputLayout = inputLayout;
        this.vs = vs;
        this.ps = ps;
        PrimitiveTopology = primitiveTopology;
        BlendState = blendState;
        RasterState = rasterState;
        DepthState = depthState;
    }
}