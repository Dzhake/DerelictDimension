#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
float Lean = 1;
float2 LinePoint;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float DistanceFromPointToLine(float2 coords, float2 linePoint, float angle)
{
    // Vector perpendicular to the line
    float2 perpendicular = float2(-sin(angle), cos(angle));
    
    // Vector from point on the line to 'coords'
    float2 toPoint = coords - linePoint;
    
    // Result
    return dot(toPoint, perpendicular);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(s0, input.TextureCoordinates);
    if (color.a != 0 && DistanceFromPointToLine(input.TextureCoordinates, LinePoint, -Lean) < abs(Lean) * 2.5)
    {
        color *= 0.5;
    }
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};