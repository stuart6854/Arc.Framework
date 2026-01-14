using Arc.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Arc.Samples.BouncingBoxes;

public sealed class Texture2D
{
    public IGfxDevice Device { get; } = null!;

    public Format Format { get; }
    public uint Width { get; }
    public uint Height { get; }

    public IGfxTextureView SRV;

    public Texture2D(IGfxDevice device, Format format, uint width, uint height, byte[]? data = null)
    {
        Device = device;
        Format = format;
        Width = width;
        Height = height;

        unsafe
        {
            fixed (byte* pData = data)
            {
                var subResourceData = new[] { new SubresourceData((IntPtr)pData, Width * Format.GetByteSize(), Width * Height * Format.GetByteSize()) };
                var texture = device.CreateTexture(
                    new TextureDesc {
                        Dimension = TextureDimension.Tex2D,
                        Format = format,
                        Width = width,
                        Height = height,
                        BindFlags = BindFlags.ShaderResource,
                    },
                    subResourceData
                )!;
                SRV = device.CreateTextureView(texture, TextureViewDesc.SRV())!;
            }
        }
    }

    public static Texture2D FromFile(IGfxDevice device, string filename)
    {
        using var image = Image.Load<Byte4>(filename);
        var bytes = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(bytes);
        return new Texture2D(device, Format.RGBA8_UNORM, (uint)image.Width, (uint)image.Height, bytes);
    }
}