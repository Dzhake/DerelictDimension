#include "Card.fxh"

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
float TotalTime;

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

float2 DeModifyCoords(float2 coords)
{
    return float2(coords.x - Lean * 0.25, coords.y);
}

float Distance(float2 coords)
{
    return sdBevelBox(coords, float2(HalfSideX, HalfSideY), CardRadius);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 coords = input.TextureCoordinates;
    //move coordinates in the way opposite to transformation. E.g., instead of moving right by 0.1 move left by 0.1
    float2 mod_coords = DeModifyCoords(coords);
    float d = Distance(mod_coords);
    float distance = Distance(coords);
    //if demodified coordinates are in the original shape, that means that after modification they'll be right in our current pixel! so we need to take original pixel (from the original shape) and use it as current pixel. That way the original pixel moves to current pixel.
    float borderSize = 0.005;
    if (abs(d) < borderSize)
    {
        return float4(0, 1, 0, smoothstep(borderSize, -borderSize, d));
    }
    if (d <= 0)
    {
        float4 color = tex2D(s0, mod_coords);
        color.a = 1;
        color.g += 2;
        return color;
    }
    //if demodified coords are not in the original shape, but normal coords are, that means that our pixel was moved. Current pixel's value [was already/will be] taken by some other pixel. This means current pixel should be empty.
    else if (distance <= 0)
    {
        return float4(0, 0, 0, 0);
    }
        
    //sample texture
    return tex2D(s0, coords);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};