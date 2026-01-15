using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arc.Graphics;
using Vortice.Mathematics;
using Rect = Vortice.Mathematics.Rect;

namespace Arc.Samples.SpaceShooter;

public static class Renderer2D
{
    public static IGfxDevice Device { get; private set; } = null!;

    private static IGfxPipeline _pipeline = null!;
    private static IGfxBuffer _cameraCB = null!;
    private static IGfxBuffer _vertexBuffer = null!;

    private static Texture2D _whiteTexture = null!;

    private static Matrix4x4 _projection = Matrix4x4.Identity;

    private const int BatchVertsPerQuad = 6;
    private const int MaxBatchQuads = 4096;
    private const int MaxBatchVertices = MaxBatchQuads * BatchVertsPerQuad;

    private readonly record struct Vertex(Vector2 Position, Vector2 TexCoord, uint Color);

    private readonly struct Batch(Texture2D texture)
    {
        public readonly Texture2D Texture = texture;
        public readonly List<Vertex> Vertices = new(MaxBatchVertices);
    }

    private static readonly LinkedList<Batch> Batches = [];

    public static void Init(IGfxDevice device)
    {
        Device = device;

        var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        string resourceName = resources.First(r => r.EndsWith("2D.hlsl"));
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!)
        using (StreamReader reader = new StreamReader(stream))
        {
            string shaderSource = reader.ReadToEnd();

            _pipeline = device.CreatePipeline(
                new PipelineDesc {
                    InputElements = [
                        new InputElementDesc("POSITION", 0, Format.RG32_FLOAT, slot: 0, alignedByteOffset: 0),
                        new InputElementDesc("TEXCOORD", 0, Format.RG32_FLOAT, slot: 0, alignedByteOffset: 8),
                        new InputElementDesc("COLOR", 0, Format.RGBA8_UNORM, slot: 0, alignedByteOffset: 16),
                    ],
                    RasterState = new RasterStateDesc {
                        FillMode = FillMode.Solid,
                        CullMode = CullMode.None,
                        ScissorEnable = true,
                        AntialiasedLineEnable = true,
                    },
                    BlendMode = BlendMode.AlphaBlend,
                    Debug = true,
                }.FromSingleString(shaderSource)
            );
        }

        _cameraCB = Device.CreateBuffer(
            new BufferDesc {
                Size = (uint)Unsafe.SizeOf<Matrix4x4>(),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
            }
        )!;

        _vertexBuffer = Device.CreateBuffer(
            new BufferDesc {
                Size = MaxBatchVertices * (uint)Unsafe.SizeOf<Vertex>(),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                StructureByteStride = (uint)Unsafe.SizeOf<Vertex>()
            }
        )!;

        var whitePixels = new byte[] { 0xff, 0xff, 0xff, 0xff };
        _whiteTexture = new Texture2D(Device, Format.RGBA8_UNORM, 1, 1, whitePixels);
    }

    public static void Shutdown()
    {
        Batches.Clear();
        _whiteTexture = null!;
        _vertexBuffer = null!;
        Device = null!;
    }

    public static void SetCamera(float width, float height) { _projection = Matrix4x4.CreateOrthographic(width, height, -1, 1); }
    public static void SetCamera(float left, float right, float top, float bottom)
    {
        _projection = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, -1, 1);
    }

    public static void DrawLine(Vector2 p0, Vector2 p1, Color4 color, float thicknessWorld)
    {
        ref var batch = ref EnsureBatch(_whiteTexture, BatchVertsPerQuad);

        var ht = thicknessWorld * 0.5f;
        var dir = Vector2.Normalize(p1 - p0);
        var perp = new Vector2(dir.Y, -dir.X) * ht;

        batch.Vertices.Add(new Vertex { Position = p0 - perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = p1 - perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = p1 + perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = p1 + perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = p0 + perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = p0 - perp, TexCoord = Vector2.Zero, Color = color.ToRgba() });
    }

    public static void DrawQuad(Vector2 center, Vector2 size, Color4 color, Texture2D? texture = null)
    {
        if (texture is null)
            texture = _whiteTexture;

        ref var batch = ref EnsureBatch(texture, BatchVertsPerQuad);

        var hs = size * 0.5f;
        var rect = new Rect(center - hs, new Size(size.X, size.Y));
        batch.Vertices.Add(new Vertex { Position = rect.TopLeft, TexCoord = new Vector2(0, 1), Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = rect.TopRight, TexCoord = new Vector2(1, 1), Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = rect.BottomRight, TexCoord = new Vector2(1, 0), Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = rect.BottomRight, TexCoord = new Vector2(1, 0), Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = rect.BottomLeft, TexCoord = new Vector2(0, 0), Color = color.ToRgba() });
        batch.Vertices.Add(new Vertex { Position = rect.TopLeft, TexCoord = new Vector2(0, 1), Color = color.ToRgba() });
    }

    private static ref Batch EnsureBatch(Texture2D texture, int requiredVertices)
    {
        if (Batches.Last is null)
            Batches.AddLast(new Batch(texture));

        ref var lastBatch = ref Batches.Last!.ValueRef;
        if (!Batches.Last.Value.Texture.Equals(texture))
            Batches.AddLast(new Batch(texture));

        if (lastBatch.Vertices.Count + requiredVertices >= MaxBatchVertices)
            Batches.AddLast(new Batch(texture));

        return ref lastBatch;
    }

    public static void Flush(IGfxCmdList cmdList)
    {
        Device.UploadData(cmdList, _cameraCB, 0, _projection);
        cmdList.SetPipeline(_pipeline);
        cmdList.SetConstantBuffer(ShaderStages.Vertex, 0, _cameraCB);
        cmdList.SetVertexBuffer(0, _vertexBuffer);
        foreach (var batch in Batches)
        {
            Device.UploadData(cmdList, _vertexBuffer, 0, batch.Vertices);

            cmdList.SetTexture(ShaderStages.Pixel, 0, batch.Texture.SRV);
            cmdList.Draw((uint)batch.Vertices.Count, 0);
        }
        Batches.Clear();
    }
}