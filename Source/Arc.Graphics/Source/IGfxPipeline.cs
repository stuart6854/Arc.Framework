namespace Arc.Graphics;

public struct InputElementDesc
{
    public string SemanticName;
    public uint SemanticIndex;
    public Format Format;
    public uint Slot;
    public uint AlignedByteOffset;
    public bool IsInstanced;

    public InputElementDesc(string semanticName, uint semanticIndex, Format format, uint slot, uint alignedByteOffset = 0, bool isInstanced = false)
    {
        SemanticName = semanticName;
        SemanticIndex = semanticIndex;
        Format = format;
        Slot = slot;
        AlignedByteOffset = alignedByteOffset;
        IsInstanced = isInstanced;
    }
}

public class ShaderDesc
{
    public string SourceCode = string.Empty;
    public string EntryPoint = string.Empty;
}

public enum CompareOp
{
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always,
}

public struct DepthStencilDesc(bool depthEnable, bool depthWriteEnable, CompareOp depthFunc, bool stencilEnable)
{
    public bool DepthEnable = depthEnable;
    public bool DepthWriteEnable = depthWriteEnable;
    public CompareOp DepthFunc = depthFunc;
    public bool StencilEnable = stencilEnable;

    public static DepthStencilDesc DepthOff => new(false, false, CompareOp.Never, false);
    public static DepthStencilDesc DepthTestWrite => new(true, true, CompareOp.LessEqual, false);
    public static DepthStencilDesc DepthTestOnly => new(true, false, CompareOp.LessEqual, false);
}

public enum PrimitiveTopology
{
    PointList,
    LineList,
    LineStrip,
    TriangleList,
    TriangleStrip,
}

public enum FillMode
{
    Solid,
    Wireframe,
}

public enum CullMode
{
    None,
    Front,
    Back,
}

public struct RasterStateDesc(CullMode cullMode, FillMode fillMode)
{
    public FillMode FillMode = fillMode;
    public CullMode CullMode = cullMode;
    public bool FrontCounterClockwise;

    public int DepthBias;
    public float DepthBiasClamp;
    public float SlopeScaledDepthBias;
    public bool DepthClipEnable = true;
    public bool ScissorEnable;
    public bool MultisampleEnable = true;
    public bool AntialiasedLineEnable;

    public static RasterStateDesc CullNone => new(CullMode.None, FillMode.Solid);
    public static RasterStateDesc CullFront => new(CullMode.Front, FillMode.Solid);
    public static RasterStateDesc CullBack => new(CullMode.Back, FillMode.Solid);
    public static RasterStateDesc Wireframe => new(CullMode.None, FillMode.Wireframe);
}

public enum BlendMode
{
    None,
    AlphaBlend,
    Additive,
    Multiply,
}

public struct PipelineDesc
{
    public InputElementDesc[] InputElements = [];
    public ShaderDesc VertexShader = new() { EntryPoint = "VS_Main" };
    public ShaderDesc PixelShader = new() { EntryPoint = "PS_Main" };

    public PrimitiveTopology PrimitiveTopology = PrimitiveTopology.TriangleList;
    public DepthStencilDesc DepthStencil = DepthStencilDesc.DepthOff;
    public RasterStateDesc RasterState = RasterStateDesc.CullNone;
    public BlendMode BlendMode = BlendMode.AlphaBlend;

    public bool Debug = false;

    public PipelineDesc() { }

    public PipelineDesc FromSingleString(string source)
    {
        try
        {
            VertexShader.SourceCode = source;
            PixelShader.SourceCode = source;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return this;
    }

    public PipelineDesc FromSingleFile(string filename)
    {
        try
        {
            var source = File.ReadAllText(filename);

            VertexShader.SourceCode = source;
            PixelShader.SourceCode = source;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return this;
    }
}

public interface IGfxPipeline { }