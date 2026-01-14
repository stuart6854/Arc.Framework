using System.Runtime.InteropServices;

namespace Arc.Graphics;

public interface IGfxDevice : IDisposable
{
    IGfxSwapchain CreateSwapchain(IntPtr hwnd, uint width, uint height);
    IGfxPipeline CreatePipeline(PipelineDesc desc);
    // TODO Better initial data handling (other function overloads?)
    IGfxBuffer? CreateBuffer(BufferDesc desc, SubresourceData? initialData = null);
    IGfxTexture? CreateTexture(TextureDesc desc, SubresourceData[]? initialData = null);
    IGfxTextureView? CreateTextureView(IGfxTexture texture, TextureViewDesc desc);
    IGfxSampler? CreateSampler(SamplerDesc desc);

    GfxRenderTarget? CreateRenderTarget(RenderTargetDesc desc)
    {
        var baseTextureDesc = new TextureDesc {
            Dimension = TextureDimension.Tex2D,
            Width = desc.Width,
            Height = desc.Height,
            Depth = 1,
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            SampleCount = 1,
            Usage = Usage.Default,
        };

        var rt = new GfxRenderTarget();
        rt.ColorTextures = new IGfxTexture[desc.ColorFormats.Length];
        rt.ColorRTVs = new IGfxTextureView[desc.ColorFormats.Length];
        for (var i = 0; i < desc.ColorFormats.Length; i++)
        {
            var texDesc = baseTextureDesc with { Format = desc.ColorFormats[i] };

            rt.ColorTextures[i] = CreateTexture(texDesc)!;
            rt.ColorRTVs[i] = CreateTextureView(rt.ColorTextures[i], TextureViewDesc.RTV())!;
        }

        if (desc.DepthFormat != Format.Unknown)
        {
            var texDesc = baseTextureDesc with { Format = desc.DepthFormat, BindFlags = BindFlags.DepthStencil };
            rt.DepthTexture = CreateTexture(texDesc);
            rt.DepthDSV = CreateTextureView(rt.DepthTexture!, TextureViewDesc.DSV())!;
        }

        return rt;
    }

    void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, T data);
    void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, ref T data);
    void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, T[] data) where T : unmanaged;
    void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, ReadOnlySpan<T> data) where T : unmanaged;
    void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, List<T> data) where T : unmanaged
    {
        UploadData(cmdList, buffer, dstOffset, CollectionsMarshal.AsSpan<T>(data));
    }

    IGfxCmdList GetImmediateCmdList(); // D3D11: maps to immediate context

    void Flush();
}