namespace Arc.Graphics;

[Flags]
public enum ShaderStages
{
    None = 0,
    Vertex = 1 << 0,
    Pixel = 1 << 1,
}

public enum IndexFormat
{
    UInt16,
    UInt32,
}

public interface IGfxCmdList : IDisposable
{
    void Begin();
    void End();

    void ClearRenderTarget(GfxRenderTarget renderTarget);
    void SetRenderTarget(GfxRenderTarget renderTarget);

    void SetViewport(Viewport viewport);
    void SetScissorRect(Rect scissorRect);

    void SetPipeline(IGfxPipeline pipeline);

    void SetConstantBuffer(ShaderStages stages, uint slot, IGfxBuffer buffer);
    void SetTexture(ShaderStages stages, uint slot, IGfxTextureView view);
    void SetSampler(ShaderStages stages, uint slot, IGfxSampler sampler);

    void SetIndexBuffer(IGfxBuffer buffer, IndexFormat format, uint offset = 0);
    void SetVertexBuffer(uint slot, IGfxBuffer buffer, uint offset = 0);

    void Draw(uint vertexCount, uint startVertexLocation);
    void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation);

    nint Map(IGfxBuffer buffer);
    void Unmap(IGfxBuffer buffer);
}