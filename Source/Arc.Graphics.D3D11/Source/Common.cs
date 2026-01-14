using Vortice.Direct3D11;
using BindFlags = Arc.Graphics.BindFlags;
using CullMode = Arc.Graphics.CullMode;
using FillMode = Arc.Graphics.FillMode;
using Filter = Arc.Graphics.Filter;
using TextureAddressMode = Arc.Graphics.TextureAddressMode;

namespace Arc.Graphics.D3D11;

internal class Common
{
    public static Vortice.DXGI.Format ToD3D11Format(Format format)
    {
        return format switch {
            Format.Unknown => Vortice.DXGI.Format.Unknown,

            Format.R16_UINT => Vortice.DXGI.Format.R16_UInt,

            Format.R32_TYPELESS => Vortice.DXGI.Format.R32_Typeless,
            Format.R32_FLOAT => Vortice.DXGI.Format.R32_Float,
            Format.R32_UINT => Vortice.DXGI.Format.R32_UInt,
            Format.R32_SINT => Vortice.DXGI.Format.R32_SInt,

            Format.RG32_FLOAT => Vortice.DXGI.Format.R32G32_Float,

            Format.RGB32_FLOAT => Vortice.DXGI.Format.R32G32B32_Float,

            Format.RGBA8_TYPELESS => Vortice.DXGI.Format.R8G8B8A8_Typeless,
            Format.RGBA8_UNORM => Vortice.DXGI.Format.R8G8B8A8_UNorm,
            Format.RGBA8_UNORM_SRGB => Vortice.DXGI.Format.R8G8B8A8_UNorm_SRgb,
            Format.RGBA8_UINT => Vortice.DXGI.Format.R8G8B8A8_UInt,
            Format.RGBA8_SNORM => Vortice.DXGI.Format.R8G8B8A8_SNorm,
            Format.RGBA8_SINT => Vortice.DXGI.Format.R8G8B8A8_SInt,

            Format.RGBA32_TYPELESS => Vortice.DXGI.Format.R32G32B32A32_Typeless,
            Format.RGBA32_FLOAT => Vortice.DXGI.Format.R32G32B32A32_Float,
            Format.RGBA32_UNORM => Vortice.DXGI.Format.R32G32B32A32_UInt,
            Format.RGBA32_SNORM => Vortice.DXGI.Format.R32G32B32A32_SInt,

            Format.BGRA8_TYPELESS => Vortice.DXGI.Format.B8G8R8A8_Typeless,
            Format.BGRA8_UNORM => Vortice.DXGI.Format.B8G8R8A8_UNorm,
            Format.BGRA8_UNORM_SRGB => Vortice.DXGI.Format.B8G8R8A8_UNorm_SRgb,

            Format.D24_UNORM_S8_UINT => Vortice.DXGI.Format.D24_UNorm_S8_UInt,
            Format.R24_UNORM_X8_TYPELESS => Vortice.DXGI.Format.R24_UNorm_X8_Typeless,

            Format.D32_FLOAT => Vortice.DXGI.Format.D32_Float,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D.PrimitiveTopology ToD3D11PrimitiveTopology(PrimitiveTopology topology)
    {
        return topology switch {
            PrimitiveTopology.PointList => Vortice.Direct3D.PrimitiveTopology.PointList,
            PrimitiveTopology.LineList => Vortice.Direct3D.PrimitiveTopology.LineList,
            PrimitiveTopology.LineStrip => Vortice.Direct3D.PrimitiveTopology.LineStrip,
            PrimitiveTopology.TriangleList => Vortice.Direct3D.PrimitiveTopology.TriangleList,
            PrimitiveTopology.TriangleStrip => Vortice.Direct3D.PrimitiveTopology.TriangleStrip,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D11.ComparisonFunction ToD3D11ComparisonFunction(CompareOp func)
    {
        return func switch {
            CompareOp.Never => ComparisonFunction.Never,
            CompareOp.Less => ComparisonFunction.Less,
            CompareOp.Equal => ComparisonFunction.Equal,
            CompareOp.LessEqual => ComparisonFunction.LessEqual,
            CompareOp.Greater => ComparisonFunction.Greater,
            CompareOp.NotEqual => ComparisonFunction.NotEqual,
            CompareOp.GreaterEqual => ComparisonFunction.GreaterEqual,
            CompareOp.Always => ComparisonFunction.Always,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D11.FillMode ToD3D11FillMode(FillMode fillMode)
    {
        return fillMode switch {
            FillMode.Solid => Vortice.Direct3D11.FillMode.Solid,
            FillMode.Wireframe => Vortice.Direct3D11.FillMode.Wireframe,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D11.CullMode ToD3D11CullMode(CullMode cullMode)
    {
        return cullMode switch {
            CullMode.None => Vortice.Direct3D11.CullMode.None,
            CullMode.Front => Vortice.Direct3D11.CullMode.Front,
            CullMode.Back => Vortice.Direct3D11.CullMode.Back,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D11.BindFlags ToD3D11BindFlags(BindFlags bindFlags)
    {
        var value = Vortice.Direct3D11.BindFlags.None;
        if (bindFlags.HasFlag(BindFlags.VertexBuffer))
            value |= Vortice.Direct3D11.BindFlags.VertexBuffer;
        if (bindFlags.HasFlag(BindFlags.IndexBuffer))
            value |= Vortice.Direct3D11.BindFlags.IndexBuffer;
        if (bindFlags.HasFlag(BindFlags.ConstantBuffer))
            value |= Vortice.Direct3D11.BindFlags.ConstantBuffer;
        if (bindFlags.HasFlag(BindFlags.ShaderResource))
            value |= Vortice.Direct3D11.BindFlags.ShaderResource;
        if (bindFlags.HasFlag(BindFlags.RenderTarget))
            value |= Vortice.Direct3D11.BindFlags.RenderTarget;
        if (bindFlags.HasFlag(BindFlags.DepthStencil))
            value |= Vortice.Direct3D11.BindFlags.DepthStencil;
        if (bindFlags.HasFlag(BindFlags.UnorderedAccess))
            value |= Vortice.Direct3D11.BindFlags.UnorderedAccess;
        return value;
    }

    public static Vortice.Direct3D11.ResourceUsage ToD3D11Usage(Usage usage)
    {
        return usage switch {
            Usage.Default => Vortice.Direct3D11.ResourceUsage.Default,
            Usage.Dynamic => Vortice.Direct3D11.ResourceUsage.Dynamic,
            Usage.Staging => Vortice.Direct3D11.ResourceUsage.Staging,
            Usage.Immutable => Vortice.Direct3D11.ResourceUsage.Immutable,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static Vortice.Direct3D11.Filter ToD3D11Filter(Filter filter)
    {
        return filter switch {
            Filter.MinMagMipPoint => Vortice.Direct3D11.Filter.MinMagMipPoint,
            Filter.MinMagMipLinear => Vortice.Direct3D11.Filter.MinMagMipLinear,
            Filter.Anisotropic => Vortice.Direct3D11.Filter.Anisotropic,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }

    public static Vortice.Direct3D11.TextureAddressMode ToD3D11AddressMode(TextureAddressMode addressMode)
    {
        return addressMode switch {
            TextureAddressMode.Wrap => Vortice.Direct3D11.TextureAddressMode.Wrap,
            TextureAddressMode.Clamp => Vortice.Direct3D11.TextureAddressMode.Clamp,
            TextureAddressMode.Border => Vortice.Direct3D11.TextureAddressMode.Border,
            TextureAddressMode.Mirror => Vortice.Direct3D11.TextureAddressMode.Mirror,
            _ => throw new ArgumentOutOfRangeException(nameof(addressMode), addressMode, null)
        };
    }
}