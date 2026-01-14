namespace Arc.Graphics;

public struct BufferDesc
{
    public uint Size { get; set; }
    public BindFlags BindFlags { get; set; }
    public Usage Usage { get; set; }
    public MiscResourceFlags MiscFlags { get; set; }
    public uint StructureByteStride { get; set; } = 0;

    public BufferDesc() { }

    public static BufferDesc Vertex(uint size, uint stride)
    {
        return new BufferDesc
        {
            Size = size,
            BindFlags = BindFlags.VertexBuffer,
            Usage = Usage.Default,
            MiscFlags = MiscResourceFlags.None,
            StructureByteStride = stride,
        };
    }
    public static BufferDesc Index(uint size, bool is32Bit)
    {
        return new BufferDesc
        {
            Size = size,
            BindFlags = BindFlags.IndexBuffer,
            Usage = Usage.Default,
            MiscFlags = MiscResourceFlags.None,
            StructureByteStride = is32Bit ? 4u : 2u,
        };
    }
    public static BufferDesc Constant(uint size, uint stride)
    {
        return new BufferDesc
        {
            Size = size,
            BindFlags = BindFlags.ConstantBuffer,
            Usage = Usage.Dynamic,
            MiscFlags = MiscResourceFlags.None,
            StructureByteStride = stride,
        };
    }
    public static BufferDesc Staging(uint size)
    {
        return new BufferDesc
        {
            Size = size,
            BindFlags = BindFlags.None,
            Usage = Usage.Staging,
            MiscFlags = MiscResourceFlags.None,
            StructureByteStride = 0u,
        };
    }
}

public interface IGfxBuffer
{
    public uint Size => Desc.Size;
    public BindFlags BindFlags => Desc.BindFlags;
    public Usage Usage => Desc.Usage;
    public MiscResourceFlags MiscFlags => Desc.MiscFlags;

    public BufferDesc Desc { get; }
}