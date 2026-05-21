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
float Lean;
float HalfSideX;
float HalfSideY;
float CardRadius;

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

bool InOriginalShape(float2 coords)
{
    float2 center = float2(0.5, 0.5);
    float distX = abs(coords.x - center.x);
    float distY = abs(coords.y - center.y);
    return distX + distY < CardRadius && distX < HalfSideX && distY < HalfSideY;
}

float2 DeModifyCoords(float2 coords)
{
    
    return float2(coords.x - Lean * 0.25, coords.y);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = float4(0, 0, 0, 1);
    float2 coords = input.TextureCoordinates;
    
    float2 demodifiedCoords = DeModifyCoords(coords);
    if (InOriginalShape(demodifiedCoords))
    {
        coords = demodifiedCoords;
    }
    else if (InOriginalShape(coords))
    {
        return float4(0, 0, 0, 0);
    }
        
    color = tex2D(s0, coords);
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};