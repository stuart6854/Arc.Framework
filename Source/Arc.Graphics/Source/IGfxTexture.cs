namespace Arc.Graphics;

public enum TextureDimension
{
    Tex1D,
    Tex2D,
    Tex3D,
}

public enum TextureViewType
{
    SRV,
    UAV,
    RTV,
    DSV,
}

public enum TextureUsage
{
    None = 0,
    ShaderRead = 1 << 0, // SRV,
    ShaderWrite = 1 << 1, // UAV
    RenderTarget = 1 << 2, // RTV/ColorAttachment
    DepthStencil = 1 << 3, // DSV/DepthAttachment
    CopySrc = 1 << 4,
    CopyDst = 1 << 4,
    GenerateMips = 1 << 5,
    Presentable = 1 << 6,
}

public struct SubresourceData
{
    /// <summary>
    /// Pointer to the texture data in memory. This raw data will be interpreted based on 
    /// the texture format and dimensions. For example, for an RGBA8 format, each pixel 
    /// will be 4 bytes (R,G,B,A channels, 8 bits each).
    /// </summary>
    public IntPtr DataPtr { get; init; }
    /// <summary>
    /// The number of bytes between the beginning of one row of pixels and the next row of pixels.
    /// Typically width * bytes-per-pixel, aligned to an implementation-defined boundary.
    /// For block compressed formats, this represents the width in bytes of one block row.
    /// </summary>
    public uint RowPitch { get; init; }
    /// <summary>
    /// The number of bytes between the beginning of one depth slice and the next depth slice.
    /// Used primarily for 3D textures, representing the size in bytes of one 2D slice of the texture.
    /// For block compressed formats, this represents the size in bytes of one compressed block slice.
    /// </summary>
    public uint SlicePitch { get; init; }

    public SubresourceData(IntPtr dataPtr, uint rowPitch = 0, uint slicePitch = 0)
    {
        DataPtr = dataPtr;
        RowPitch = rowPitch;
        SlicePitch = slicePitch;
    }
}

public struct TextureDesc
{
    public TextureDimension Dimension { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; } = 1; // 1 = 1D
    public uint Depth { get; init; } = 1; // 1 = 1D/2D
    /// 0 = Full chain
    public uint MipLevels { get; init; } = 1;
    public uint ArraySize { get; init; } = 1;
    public uint SampleCount { get; init; } = 1; // 1 = Non-MSAA
    public Format Format { get; init; }
    public Usage Usage { get; init; }
    public BindFlags BindFlags { get; init; }
    public MiscResourceFlags MiscFlags { get; init; }

    public TextureDesc() { }
}

public interface IGfxTexture
{
    public TextureDimension Dimension => Desc.Dimension;
    public uint Width => Desc.Width;
    public uint Height => Desc.Height;
    public uint Depth => Desc.Depth;
    public uint MipLevels => Desc.MipLevels;
    public uint ArraySize => Desc.ArraySize;
    public uint SampleCount => Desc.SampleCount;
    public Format Format => Desc.Format;
    public Usage Usage => Desc.Usage;
    public BindFlags BindFlags => Desc.BindFlags;
    public MiscResourceFlags MiscFlags => Desc.MiscFlags;

    public TextureDesc Desc { get; }
}

public struct TextureViewDesc
{
    public TextureViewType ViewType { get; init; }
    /// Unknown to inherit from texture
    public Format FormatOverride { get; init; } = Format.Unknown;
    public uint BaseMip { get; init; } = 0;
    public uint MipCount { get; init; } = 1;
    public uint BaseLayer { get; init; } = 0;
    public uint LayerCount { get; init; } = 1;

    public TextureViewDesc() { }

    public static TextureViewDesc SRV(uint baseMip = 0, uint mipCount = ~0u, uint baseLayer = 0, uint layerCount = ~0u)
    {
        return new TextureViewDesc
        {
            ViewType = TextureViewType.SRV,
            BaseMip = baseMip,
            MipCount = mipCount,
            BaseLayer = baseLayer,
            LayerCount = layerCount,
        };
    }
    public static TextureViewDesc UAV(uint baseMip = 0, uint mipCount = ~0u, uint baseLayer = 0, uint layerCount = ~0u)
    {
        return new TextureViewDesc
        {
            ViewType = TextureViewType.UAV,
            BaseMip = baseMip,
            MipCount = mipCount,
            BaseLayer = baseLayer,
            LayerCount = layerCount,
        };
    }
    public static TextureViewDesc RTV(uint baseMip = 0, uint mipCount = ~0u, uint baseLayer = 0, uint layerCount = ~0u)
    {
        return new TextureViewDesc
        {
            ViewType = TextureViewType.RTV,
            BaseMip = baseMip,
            MipCount = mipCount,
            BaseLayer = baseLayer,
            LayerCount = layerCount,
        };
    }
    public static TextureViewDesc DSV(uint baseMip = 0, uint mipCount = ~0u, uint baseLayer = 0, uint layerCount = ~0u)
    {
        return new TextureViewDesc
        {
            ViewType = TextureViewType.DSV,
            BaseMip = baseMip,
            MipCount = mipCount,
            BaseLayer = baseLayer,
            LayerCount = layerCount,
        };
    }
}

public interface IGfxTextureView
{
    TextureViewDesc Desc { get; }

    public bool IsSRV => Desc.ViewType == TextureViewType.SRV;
    public bool IsUAV => Desc.ViewType == TextureViewType.UAV;
    public bool IsRTV => Desc.ViewType == TextureViewType.RTV;
    public bool IsDSV => Desc.ViewType == TextureViewType.DSV;
}