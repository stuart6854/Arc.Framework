namespace Arc.Graphics;

public enum Filter
{
    MinMagMipPoint,
    MinMagMipLinear,
    Anisotropic,
}

public enum TextureAddressMode
{
    Wrap,
    Clamp,
    Border,
    Mirror,
}

public struct SamplerDesc
{
    public Filter Filter { get; set; }
    public TextureAddressMode AddressU { get; set; }
    public TextureAddressMode AddressV { get; set; }
    public TextureAddressMode AddressW { get; set; }
    public float MipLodBias { get; set; }
    public uint MaxAnisotropy { get; set; }
    public float MinLod { get; set; }
    public float MaxLod { get; set; }

    public SamplerDesc(Filter filter, TextureAddressMode addressMode, float mipLodBias = 0.0f, uint maxAnisotropy = 1, float minLod = float.MinValue, float maxLod = float.MaxValue)
    {
        Filter = filter;
        AddressU = addressMode;
        AddressV = addressMode;
        AddressW = addressMode;
        MipLodBias = mipLodBias;
        MaxAnisotropy = maxAnisotropy;
        MinLod = minLod;
        MaxLod = maxLod;
    }

    public static SamplerDesc PointWrap => new SamplerDesc(Filter.MinMagMipPoint, TextureAddressMode.Wrap);
    public static SamplerDesc PointClamp => new SamplerDesc(Filter.MinMagMipPoint, TextureAddressMode.Clamp);
    public static SamplerDesc LinearWrap => new SamplerDesc(Filter.MinMagMipLinear, TextureAddressMode.Wrap);
    public static SamplerDesc LinearClamp => new SamplerDesc(Filter.MinMagMipLinear, TextureAddressMode.Clamp);
    public static SamplerDesc AnisotropicWrap => new SamplerDesc(Filter.Anisotropic, TextureAddressMode.Wrap, maxAnisotropy: 16u);
    public static SamplerDesc AnisotropicClamp => new SamplerDesc(Filter.Anisotropic, TextureAddressMode.Clamp, maxAnisotropy: 16u);
}

public interface IGfxSampler { }