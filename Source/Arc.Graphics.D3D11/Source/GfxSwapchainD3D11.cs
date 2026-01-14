using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Arc.Graphics.D3D11;

public sealed class GfxSwapchainD3D11 : IGfxSwapchain
{
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public GfxRenderTarget CurrentTarget { get; private set; } = null!;

    private readonly ID3D11Device device;
    private readonly IDXGISwapChain3 swapchain;
    private GfxRenderTarget renderTarget = null!;

    public GfxSwapchainD3D11(ID3D11Device device, IDXGISwapChain3 swapchain)
    {
        this.device = device;
        this.swapchain = swapchain;
        Width = swapchain.Description1.Width;
        Height = swapchain.Description1.Height;

        RecreateRenderTargets();
    }

    public void Resize(uint width, uint height)
    {
        var hr = swapchain.ResizeBuffers(0, width, height);
        hr.CheckError();

        Width = width;
        Height = height;

        RecreateRenderTargets();
    }

    private void RecreateRenderTargets()
    {
        var buffer = swapchain.GetBuffer<ID3D11Texture2D>(0);
        var target = device.CreateRenderTargetView(buffer);

        var rtv = new GfxTextureViewD3D11(target, TextureViewDesc.RTV());

        renderTarget = new GfxRenderTarget {
            ColorRTVs = [rtv],
        };

        CurrentTarget = renderTarget;
    }

    public void Present(uint syncInterval)
    {
        var result = swapchain.Present(syncInterval, PresentFlags.None);
        result.CheckError();
    }

    public void Dispose() { swapchain.Dispose(); }
}