struct VSInput
{
    float2 position : POSITION;
    float2 texCoord : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : SV_Position;
    float4 color : COLOR;
    float2 texCoord : TEXCOORD;
};

cbuffer Camera : register(b0)
{
    float4x4 proj;
    // float4x4 view;
};

Texture2D Texture : register(t0);
SamplerState Sampler : register(s0);

VSOutput VS_Main(VSInput input)
{
    VSOutput output;

    float4 world = float4(input.position, 0.0, 1.0);
    // output.position = mul(proj, mul(view, world));
    output.position = mul(proj, world);
    output.color = input.color;
    output.texCoord = input.texCoord;

    return output;
}

float4 PS_Main(VSOutput input) : SV_Target
{
    return input.color * Texture.Sample(Sampler, input.texCoord);
}
