float4x4 World;
float4x4 View;
float4x4 Projection;

#include "Lights.fx"

float Slime = 1;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = <Texture>;

    MinFilter = LINEAR;
    MagFilter = LINEAR;
    
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
	float4 Pos1 : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.TexCoord = input.TexCoord;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Pos1 = output.Position;

    output.Color = ComputeColor(worldPosition, input.Normal);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Compute a value that ranges from -1 to 1, where -1 is the bottom of 
	// the screen and 1 is the top.
	float y = input.Pos1.y / input.Pos1.w;   
	
	// (y - Slime) > 0 above the slime line. The * 5 makes the line faded rather
	// than a hard line.  
	float sy = saturate((y - Slime) * 5);

	// Compute the slime color
	float slime = sy * float4(0.4, 1.0, 0.4, 1) + (1 - sy) * float4(1, 1, 1, 1);

	// Output color multiplied by the slime color
    //return input.Color * tex2D(Sampler, input.TexCoord) * slime;

    return input.Color * tex2D(Sampler, input.TexCoord) * float4(Gain, 1) * slime;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
