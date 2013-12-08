/*
* This is Owen's code. All I did was turn it blue
*/

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 Eye;

float3 Light1Location;
float4 LightColor = float4(1, 1, 1, 1);
float4 LightAmbientColor = float4(0.2, 0.2, 0.2, 1);

// ***** material properties *****


float4 DiffuseColor = float4(0.34f, 0.6f, 0.8f, 1);
float4 SpecularColor = float4(0.3f, 0.3f, 0.3f, 1);

float SpecularPower = 4;

struct VS_INPUT
{
    float4 position            : POSITION0;
    float3 normal              : NORMAL0;    
};

struct VS_OUTPUT
{
    float4 position            : POSITION0;
    float3 normal              : TEXCOORD2;
    float4 worldPosition       : TEXCOORD1;
};

VS_OUTPUT VertexShaderFunc( VS_INPUT input )
{
    VS_OUTPUT output;
    
    // transform the position into projection space
    output.worldPosition = mul(input.position, World);
    output.position = mul(mul(mul(input.position, World), View), Projection);
	output.normal = normalize(mul(input.normal, World));
	    
    return output;
}

float4 PixelShaderNo( VS_OUTPUT input ) : COLOR0
{  
    float3 N = input.normal;
    
    float3 V = normalize(Eye - input.worldPosition);
    float3 L = normalize(Light1Location - input.worldPosition);
    
    float4 diffuse = LightColor * max(dot(N, L), 0);
    
    float3 H = normalize(V + L);
    
    float4 specular = pow(saturate(dot(N, H)), SpecularPower) * LightColor * SpecularColor;
    
    // return the combined result.
    return (diffuse + LightAmbientColor) * DiffuseColor + specular;
}

Technique Regular
{
    Pass Go
    {
        VertexShader = compile vs_1_1 VertexShaderFunc();
        PixelShader = compile ps_2_0 PixelShaderNo();
    }
}
