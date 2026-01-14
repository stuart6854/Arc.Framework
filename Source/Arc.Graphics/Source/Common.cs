namespace Arc.Graphics;

public enum Format
{
    Unknown,

    R16_UINT,

    R32_TYPELESS,
    R32_FLOAT,
    R32_UINT,
    R32_SINT,

    RG32_FLOAT,

    RGB32_FLOAT,

    RGBA8_TYPELESS,
    RGBA8_UNORM,
    RGBA8_UNORM_SRGB,
    RGBA8_UINT,
    RGBA8_SNORM,
    RGBA8_SINT,

    RGBA32_TYPELESS,
    RGBA32_FLOAT,
    RGBA32_UNORM,
    RGBA32_SNORM,

    BGRA8_TYPELESS,
    BGRA8_UNORM,
    BGRA8_UNORM_SRGB,

    D24_UNORM_S8_UINT,
    R24_UNORM_X8_TYPELESS,

    D32_FLOAT,
}

public readonly struct Viewport
{
    public readonly float X;
    public readonly float Y;
    public readonly float Width;
    public readonly float Height;
    public readonly float MinDepth;
    public readonly float MaxDepth;

    public Viewport(float x, float y, float width, float height, float minDepth = 0.0f, float maxDepth = 0.0f)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
    }

    public Viewport(float Width, float Height) : this(0, 0, Width, Height, 0, 1) { }
}

public readonly struct Rect
{
    public readonly int Left;
    public readonly int Top;
    public readonly int Right;
    public readonly int Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Rect(int width, int height) : this(0, 0, width, height) { }
}

#region Resources

public enum Usage
{
    /// <summary>
    /// A resource that requires read/write access for the GPU.
    /// Likely to be the most common usage choice.
    /// </summary>
    Default,
    /// <summary>
    /// A resource that can only be read by the GPU. It cannot be written by the GPU,
    /// and cannot be accessed at all by the CPU. This type of resource must be initialised
    /// when it is created, since it cannot be changed after creation.
    /// </summary>
    Immutable,
    /// <summary>
    /// A resource that is accessible by both the GPU (read only) and the CPU (write only).
    /// A dynamic resource is a good choice for a resource that will be updated by the CPU
    /// at least once per frame.
    /// </summary>
    Dynamic,
    /// <summary>
    /// A resource that supports data transfer (copy) to and from the GPU and CPU.
    /// </summary>
    Staging,
    /// <summary>
    /// 
    /// </summary>
    Readback,
}

[Flags]
public enum BindFlags
{
    None = 0,
    VertexBuffer = 1 << 0,
    IndexBuffer = 1 << 1,
    ConstantBuffer = 1 << 2,
    ShaderResource = 1 << 3,
    RenderTarget = 1 << 4,
    DepthStencil = 1 << 5,
    UnorderedAccess = 1 << 6,
}

[Flags]
public enum MiscResourceFlags
{
    None = 0,
}

#endregion

public static class CommonExtensions
{
    public static uint GetByteSize(this Format format)
    {
        switch (format)
        {
            case Format.Unknown:
                return 0;
            case Format.R16_UINT:
                return 2;
            case Format.R32_TYPELESS:
            case Format.R32_FLOAT:
            case Format.R32_UINT:
            case Format.R32_SINT:
                return 4;
            case Format.RG32_FLOAT:
            case Format.RGB32_FLOAT:
                return 8;
            case Format.RGBA8_TYPELESS:
            case Format.RGBA8_UNORM:
            case Format.RGBA8_UNORM_SRGB:
            case Format.RGBA8_UINT:
            case Format.RGBA8_SNORM:
            case Format.RGBA8_SINT:
                return 4;
            case Format.RGBA32_TYPELESS:
            case Format.RGBA32_FLOAT:
            case Format.RGBA32_UNORM:
            case Format.RGBA32_SNORM:
                return 16;
            case Format.BGRA8_TYPELESS:
            case Format.BGRA8_UNORM:
            case Format.BGRA8_UNORM_SRGB:
                return 16;
            case Format.D24_UNORM_S8_UINT:
            case Format.R24_UNORM_X8_TYPELESS:
                return 8;
            case Format.D32_FLOAT:
                return 8;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}