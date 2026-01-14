using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Arc.Graphics.D3D11;

public sealed class GfxCmdListD3D11 : IGfxCmdList
{
    internal ID3D11DeviceContext Ctx { get; private init; }

    internal GfxCmdListD3D11(ID3D11DeviceContext ctx) { Ctx = ctx; }

    public void Dispose() { Ctx.Dispose(); }

    public void Begin()
    {
        /* NOOP */
    }

    public void End()
    {
        /* NOOP */
    }

    public void ClearRenderTarget(GfxRenderTarget renderTarget)
    {
        foreach (var rt in renderTarget.ColorRTVs)
        {
            var rtv = (ID3D11RenderTargetView)((GfxTextureViewD3D11)rt).View;
            Ctx.ClearRenderTargetView(rtv, new Color4(0.2f, 0.3f, 0.3f));
        }

        if (renderTarget.DepthDSV != null)
        {
            var rtv = (ID3D11DepthStencilView)((GfxTextureViewD3D11)renderTarget.DepthDSV).View;
            Ctx.ClearDepthStencilView(rtv, DepthStencilClearFlags.Depth, 1.0f, 0);
        }
    }

    public void SetRenderTarget(GfxRenderTarget renderTarget)
    {
        var rtvs = renderTarget.ColorRTVs.Select(rt => (ID3D11RenderTargetView)((GfxTextureViewD3D11)rt).View).ToArray();
        var dsv = renderTarget.DepthDSV != null ? (ID3D11DepthStencilView)((GfxTextureViewD3D11)renderTarget.DepthDSV).View : null;

        Ctx.OMSetRenderTargets(rtvs.AsSpan(), dsv);
    }

    public void SetViewport(Viewport viewport) { Ctx.RSSetViewport(viewport); }

    public void SetScissorRect(Rect scissorRect) { Ctx.RSSetScissorRect(scissorRect); }

    public void SetPipeline(IGfxPipeline pipeline)
    {
        var vsShader = (pipeline as GfxPipelineD3D11)!.vs;
        var psShader = (pipeline as GfxPipelineD3D11)!.ps;

        Ctx.VSSetShader(vsShader);
        Ctx.PSSetShader(psShader);
        Ctx.IASetInputLayout((pipeline as GfxPipelineD3D11)!.inputLayout);
        Ctx.IASetPrimitiveTopology((pipeline as GfxPipelineD3D11)!.PrimitiveTopology);
        Ctx.OMSetBlendState((pipeline as GfxPipelineD3D11)!.BlendState);
        Ctx.OMSetDepthStencilState((pipeline as GfxPipelineD3D11)!.DepthState, 1);
        Ctx.RSSetState((pipeline as GfxPipelineD3D11)!.RasterState);
    }

    public void SetConstantBuffer(ShaderStages stages, uint slot, IGfxBuffer buffer)
    {
        if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            Ctx.VSSetConstantBuffer(slot, ((GfxBufferD3D11)buffer).Buffer);
        if ((stages & ShaderStages.Pixel) == ShaderStages.Pixel)
            Ctx.PSSetConstantBuffer(slot, ((GfxBufferD3D11)buffer).Buffer);
    }

    public void SetTexture(ShaderStages stages, uint slot, IGfxTextureView view)
    {
        if (!view.IsSRV)
            return; // TODO Error and/or Throw

        var srv = (ID3D11ShaderResourceView)(view as GfxTextureViewD3D11)!.View;

        if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            Ctx.VSSetShaderResource(slot, srv);
        if ((stages & ShaderStages.Pixel) == ShaderStages.Pixel)
            Ctx.PSSetShaderResource(slot, srv);
    }

    public void SetSampler(ShaderStages stages, uint slot, IGfxSampler sampler)
    {
        if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            Ctx.VSSetSampler(slot, (sampler as GfxSamplerD3D11)!.SamplerState);
        if ((stages & ShaderStages.Pixel) == ShaderStages.Pixel)
            Ctx.PSSetSampler(slot, (sampler as GfxSamplerD3D11)!.SamplerState);
    }

    public void SetIndexBuffer(IGfxBuffer buffer, IndexFormat format, uint offset)
    {
        var indexFormat = Common.ToD3D11Format(format == IndexFormat.UInt16 ? Format.R16_UINT : Format.R32_UINT);
        Ctx.IASetIndexBuffer(((GfxBufferD3D11)buffer).Buffer, indexFormat, offset);
    }

    public void SetVertexBuffer(uint slot, IGfxBuffer buffer, uint offset)
    {
        Ctx.IASetVertexBuffer(
            slot,
            ((GfxBufferD3D11)buffer).Buffer,
            buffer.Desc.StructureByteStride,
            offset
        );
    }

    public void Draw(uint vertexCount, uint startVertexLocation) { Ctx.Draw(vertexCount, startVertexLocation); }

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        Ctx.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }

    public nint Map(IGfxBuffer buffer)
    {
        var d3dBuffer = ((GfxBufferD3D11)buffer).Buffer;
        var map = Ctx.Map(d3dBuffer, MapMode.WriteDiscard);
        return map.DataPointer;
    }

    public void Unmap(IGfxBuffer buffer) { Ctx.Unmap(((GfxBufferD3D11)buffer).Buffer, 0); }
}