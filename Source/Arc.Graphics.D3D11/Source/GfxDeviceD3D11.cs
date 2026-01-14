using System.Runtime.CompilerServices;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using Vortice.DXGI.Debug;
using MapFlags = Vortice.Direct3D11.MapFlags;

namespace Arc.Graphics.D3D11;

public sealed class GfxDeviceD3D11 : IGfxDevice
{
    private readonly IDXGIFactory2 factory;
    private readonly ID3D11Device5 device;

    private readonly GfxCmdListD3D11 immediateCmdList;

    public static GfxDeviceD3D11 Create(bool debug = false)
    {
        var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(debug);

        var flags = debug ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;
        var device = Vortice.Direct3D11.D3D11.D3D11CreateDevice(DriverType.Hardware, flags, FeatureLevel.Level_11_1);

        if (debug)
        {
            using var d3dDebug = device.QueryInterfaceOrNull<ID3D11Debug>();
            if (d3dDebug != null)
            {
                // TODO Debug layer was not activated. Log warning
            }

            // Setup D3D11 InfoQueue to break on serious issues and collect messages
            using var d3dInfoQueue = device.QueryInterfaceOrNull<ID3D11InfoQueue>();
            if (d3dInfoQueue != null)
            {
                d3dInfoQueue.SetBreakOnSeverity(MessageSeverity.Corruption, true);
                d3dInfoQueue.SetBreakOnSeverity(MessageSeverity.Error, true);
                // Optional noise reduction
                // d3dInfoQueue.SetBreakOnSeverity(MessageSeverity.Warning, false);
            }

            // Enable DXGI debug messages too (swapchain, present, etc.)
            using var dxgiInfoQueue = DXGI.DXGIGetDebugInterface1<IDXGIInfoQueue>();
            if (dxgiInfoQueue != null)
            {
                dxgiInfoQueue.SetBreakOnSeverity(DXGI.DebugAll, InfoQueueMessageSeverity.Corruption, true);
                dxgiInfoQueue.SetBreakOnSeverity(DXGI.DebugAll, InfoQueueMessageSeverity.Error, true);
            }
        }


        return new GfxDeviceD3D11(factory, device.QueryInterface<ID3D11Device5>());
    }

    internal GfxDeviceD3D11(IDXGIFactory2 factory, ID3D11Device5 device)
    {
        this.factory = factory;
        this.device = device;
        immediateCmdList = new GfxCmdListD3D11(device.ImmediateContext);
    }

    public IGfxSwapchain CreateSwapchain(IntPtr hwnd, uint width, uint height)
    {
        var desc = new SwapChainDescription1 {
            Width = width,
            Height = height,
            Format = Vortice.DXGI.Format.R8G8B8A8_UNorm,
            BufferUsage = Vortice.DXGI.Usage.RenderTargetOutput,
            BufferCount = 2,
            SampleDescription = SampleDescription.Default,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore,
            // Flags = SwapChainFlags.AllowModeSwitch,
        };

        var fullscreenDesc = new SwapChainFullscreenDescription {
            Windowed = true,
        };

        var swapchain = factory.CreateSwapChainForHwnd(device, hwnd, desc, fullscreenDesc, null);

        // factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);

        return new GfxSwapchainD3D11(device, swapchain.QueryInterface<IDXGISwapChain3>());
    }

    public IGfxPipeline CreatePipeline(PipelineDesc desc)
    {
        var vertexShader = CreateVertexShader(desc.VertexShader.SourceCode, desc.VertexShader.EntryPoint, desc.InputElements, out var inputLayout);
        var pixelShader = CreatePixelShader(desc.PixelShader.SourceCode, desc.PixelShader.EntryPoint);

        /*var rtBlendDesc = new RenderTargetBlendDescription
        {
            BlendEnable = desc.BlendMode != BlendMode.None,
            BlendOperation = BlendOperation.Add,
            BlendOperationAlpha = BlendOperation.Add,
            DestinationBlend = Blend.InverseSourceAlpha,
            DestinationBlendAlpha = Blend.InverseSourceAlpha,
            SourceBlend = Blend.SourceAlpha,
            SourceBlendAlpha = Blend.SourceAlpha,
            RenderTargetWriteMask = ColorWriteEnable.All,
        };

        var blendDesc = new BlendDescription();
        blendDesc.AlphaToCoverageEnable = false;
        for (var i = 0; i < 8; i++)
            blendDesc.RenderTarget[i] = rtBlendDesc;*/

        var blendState = device.CreateBlendState(BlendDescription.NonPremultiplied);

        var primitiveType = Common.ToD3D11PrimitiveTopology(desc.PrimitiveTopology);

        var depthDesc = new DepthStencilDescription {
            DepthEnable = desc.DepthStencil.DepthEnable,
            DepthWriteMask = desc.DepthStencil.DepthWriteEnable ? DepthWriteMask.All : DepthWriteMask.Zero,
            DepthFunc = Common.ToD3D11ComparisonFunction(desc.DepthStencil.DepthFunc),
            StencilEnable = desc.DepthStencil.StencilEnable,
        };
        var depthState = device.CreateDepthStencilState(depthDesc);

        var rasterDesc = new RasterizerDescription {
            FillMode = Common.ToD3D11FillMode(desc.RasterState.FillMode),
            CullMode = Common.ToD3D11CullMode(desc.RasterState.CullMode),
            FrontCounterClockwise = desc.RasterState.FrontCounterClockwise,
            DepthBias = desc.RasterState.DepthBias,
            DepthBiasClamp = desc.RasterState.DepthBiasClamp,
            SlopeScaledDepthBias = desc.RasterState.SlopeScaledDepthBias,
            DepthClipEnable = desc.RasterState.DepthClipEnable,
            ScissorEnable = desc.RasterState.ScissorEnable,
            MultisampleEnable = desc.RasterState.MultisampleEnable,
            AntialiasedLineEnable = desc.RasterState.AntialiasedLineEnable,
        };
        var rasterState = device.CreateRasterizerState(rasterDesc);

        return new GfxPipelineD3D11(inputLayout, vertexShader, pixelShader, primitiveType, blendState, rasterState, depthState);
    }

    public IGfxBuffer? CreateBuffer(BufferDesc desc, SubresourceData? initialData = null)
    {
        Vortice.Direct3D11.SubresourceData? initData = null;
        if (initialData != null)
        {
            initData = new Vortice.Direct3D11.SubresourceData {
                DataPointer = initialData!.Value.DataPtr,
                RowPitch = initialData!.Value.RowPitch,
                SlicePitch = initialData!.Value.SlicePitch,
            };
        }

        var cpuAccess = desc.Usage switch {
            Usage.Dynamic => CpuAccessFlags.Write,
            Usage.Staging => CpuAccessFlags.Read | CpuAccessFlags.Write,
            _ => CpuAccessFlags.None
        };

        var bufferDesc = new BufferDescription {
            ByteWidth = desc.Size,
            BindFlags = Common.ToD3D11BindFlags(desc.BindFlags),
            Usage = Common.ToD3D11Usage(desc.Usage),
            CPUAccessFlags = cpuAccess,
            MiscFlags = 0, // TODO MiscResourceFlags
            StructureByteStride = desc.StructureByteStride,
        };

        var buffer = device.CreateBuffer(bufferDesc, initData);

        return new GfxBufferD3D11(buffer, desc);
    }

    public IGfxTexture? CreateTexture(TextureDesc desc, SubresourceData[]? initialData = null)
    {
        var castInitialData = Unsafe.As<Vortice.Direct3D11.SubresourceData[]>(initialData);

        var cpuAccess = desc.Usage switch {
            Usage.Dynamic => CpuAccessFlags.Write,
            Usage.Staging => CpuAccessFlags.Read | CpuAccessFlags.Write,
            _ => CpuAccessFlags.None
        };

        if (desc.Dimension == TextureDimension.Tex1D)
        {
            throw new NotImplementedException();
        }
        if (desc.Dimension == TextureDimension.Tex2D)
        {
            var texture = device.CreateTexture2D(
                Common.ToD3D11Format(desc.Format),
                desc.Width,
                desc.Height,
                desc.ArraySize,
                desc.MipLevels,
                castInitialData,
                Common.ToD3D11BindFlags(desc.BindFlags),
                0, // TODO MiscResourceFlags
                Common.ToD3D11Usage(desc.Usage),
                cpuAccess
            );
            return new GfxTextureD3D11(texture, desc);
        }
        if (desc.Dimension == TextureDimension.Tex3D)
        {
            throw new NotImplementedException();
        }

        return null;
    }

    public IGfxTextureView? CreateTextureView(IGfxTexture texture, TextureViewDesc desc)
    {
        var resource = ((GfxTextureD3D11)texture).Resource;
        var texDesc = texture.Desc;

        var viewFormat = Common.ToD3D11Format(desc.FormatOverride != Format.Unknown ? desc.FormatOverride : texDesc.Format);

        if (desc.ViewType == TextureViewType.SRV)
        {
            ShaderResourceViewDescription viewDesc = new();
            if (texDesc.Dimension == TextureDimension.Tex1D)
            {
                viewDesc = new ShaderResourceViewDescription(
                    (ID3D11Texture1D)resource,
                    isArray: false,
                    viewFormat,
                    mostDetailedMip: desc.BaseMip,
                    mipLevels: desc.MipCount,
                    firstArraySlice: desc.BaseLayer,
                    arraySize: desc.LayerCount
                );
            }
            else if (texDesc.Dimension == TextureDimension.Tex2D)
            {
                viewDesc = new ShaderResourceViewDescription(
                    (ID3D11Texture2D)resource,
                    // Can be Texture2D, Texture2DMultisampled, Texture2DArray, Texture2DMultisampledArray, TextureCubeArray
                    ShaderResourceViewDimension.Texture2D,
                    viewFormat,
                    mostDetailedMip: desc.BaseMip,
                    mipLevels: desc.MipCount,
                    firstArraySlice: desc.BaseLayer,
                    arraySize: desc.LayerCount
                );
            }
            else if (texDesc.Dimension == TextureDimension.Tex3D)
            {
                viewDesc = new ShaderResourceViewDescription(
                    (ID3D11Texture3D)resource,
                    viewFormat,
                    mostDetailedMip: desc.BaseMip,
                    mipLevels: desc.MipCount
                );
            }

            var view = device.CreateShaderResourceView(resource, viewDesc);
            return new GfxTextureViewD3D11(view, desc);
        }
        if (desc.ViewType == TextureViewType.UAV)
        {
            throw new NotImplementedException();
        }
        if (desc.ViewType == TextureViewType.RTV)
        {
            RenderTargetViewDescription viewDesc = new();
            var view = device.CreateRenderTargetView(resource, viewDesc);
            return new GfxTextureViewD3D11(view, desc);
        }
        if (desc.ViewType == TextureViewType.DSV)
        {
            DepthStencilViewDescription viewDesc = new();
            var view = device.CreateDepthStencilView(resource, viewDesc);
            return new GfxTextureViewD3D11(view, desc);
        }

        return null;
    }

    public IGfxSampler? CreateSampler(SamplerDesc desc)
    {
        var samplerDesc = new SamplerDescription {
            Filter = Common.ToD3D11Filter(desc.Filter),
            AddressU = Common.ToD3D11AddressMode(desc.AddressU),
            AddressV = Common.ToD3D11AddressMode(desc.AddressV),
            AddressW = Common.ToD3D11AddressMode(desc.AddressW),
            MipLODBias = desc.MipLodBias,
            MaxAnisotropy = desc.MaxAnisotropy,
            MinLOD = desc.MinLod,
            MaxLOD = desc.MaxLod,
        };

        var samplerState = device.CreateSamplerState(samplerDesc);
        return new GfxSamplerD3D11(samplerState);
    }

    public void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, T data)
    {
        var cmd = (GfxCmdListD3D11)cmdList;
        var buf = (GfxBufferD3D11)buffer;

        unsafe
        {
            var mapped = cmd.Ctx.Map(buf.Buffer, MapMode.WriteDiscard);
            var offsetPtr = IntPtr.Add(mapped.DataPointer, (int)dstOffset).ToPointer();
            Unsafe.Copy(offsetPtr, ref data);
            cmd.Ctx.Unmap(buf.Buffer, 0);
        }
    }

    public void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, ref T data)
    {
        var cmd = (GfxCmdListD3D11)cmdList;
        var buf = (GfxBufferD3D11)buffer;

        unsafe
        {
            var mapped = cmd.Ctx.Map(buf.Buffer, MapMode.WriteDiscard);
            var offsetPtr = IntPtr.Add(mapped.DataPointer, (int)dstOffset).ToPointer();
            Unsafe.Copy(offsetPtr, ref data);
            cmd.Ctx.Unmap(buf.Buffer, 0);
        }
    }

    public void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, T[] data) where T : unmanaged
    {
        var cmd = (GfxCmdListD3D11)cmdList;
        var buf = (GfxBufferD3D11)buffer;

        buf.Buffer.SetData<T>(cmd.Ctx, data, MapMode.WriteDiscard, MapFlags.None, (int)dstOffset);
    }

    public void UploadData<T>(IGfxCmdList cmdList, IGfxBuffer buffer, uint dstOffset, ReadOnlySpan<T> data) where T : unmanaged
    {
        var cmd = (GfxCmdListD3D11)cmdList;
        var buf = (GfxBufferD3D11)buffer;

        unsafe
        {
            var mapped = cmd.Ctx.Map(buf.Buffer, MapMode.WriteDiscard);
            try
            {
                var offsetPtr = IntPtr.Add(mapped.DataPointer, (int)dstOffset).ToPointer();
                var dst = new Span<T>(offsetPtr, data.Length);
                data.CopyTo(dst);
            }
            finally
            {
                cmd.Ctx.Unmap(buf.Buffer, 0);
            }
        }
    }

    public IGfxCmdList GetImmediateCmdList() { return immediateCmdList; }

    public void Flush() { }

    public void Dispose()
    {
        factory.Dispose();
        device.Dispose();
    }

    #region Shaders

    private static ReadOnlyMemory<byte> CompileBytecode(string source, string entrypoint, string targetProfile, bool debug = false)
    {
        var sourceName = "<shader_source>";
        var flags = ShaderFlags.EnableStrictness | (debug ? ShaderFlags.Debug | ShaderFlags.SkipOptimization : ShaderFlags.OptimizationLevel3);

        var result = Compiler.Compile(source, entrypoint, sourceName, targetProfile, out var bytecode, out var errors);
        if (result.Failure)
        {
            throw new Exception($"Failed to compile shader:\n{errors.AsString()}");
        }

        return bytecode.AsMemory();
    }

    private const string ShaderProfileVersion = "5_0";

    private ID3D11VertexShader CreateVertexShader(string source, string entryPoint, InputElementDesc[] inputElements, out ID3D11InputLayout inputLayout)
    {
        var d3dInputElements = new InputElementDescription[inputElements.Length];
        for (var i = 0; i < d3dInputElements.Length; i++)
        {
            d3dInputElements[i] = new InputElementDescription(
                inputElements[i].SemanticName,
                inputElements[i].SemanticIndex,
                Common.ToD3D11Format(inputElements[i].Format),
                inputElements[i].AlignedByteOffset,
                inputElements[i].Slot,
                inputElements[i].IsInstanced ? InputClassification.PerInstanceData : InputClassification.PerVertexData,
                inputElements[i].IsInstanced ? 1u : 0u
            );
        }

        var bytecode = CompileBytecode(source, entryPoint, $"vs_{ShaderProfileVersion}");
        inputLayout = d3dInputElements.Length > 0 ? device.CreateInputLayout(d3dInputElements, bytecode.Span) : null!;
        return device.CreateVertexShader(bytecode.Span);
    }

    private ID3D11PixelShader CreatePixelShader(string source, string entryPoint)
    {
        var bytecode = CompileBytecode(source, entryPoint, $"ps_{ShaderProfileVersion}");
        return device.CreatePixelShader(bytecode.Span);
    }

    #endregion
}