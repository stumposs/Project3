
// TODO: add effect parameters here.
float3 DiffuseColor;

float3 LightAmbient = float3(0.05, 0.05, 0.10);
float3 Light1Location = float3(568, 246, 1036);
float3 Light1Color = float3(1, 1, 1);
float3 Light2Location = float3(821, 224, 941);
float3 Light2Color = float3(14.29, 45, 43.94);
float3 Light3Location = float3(824, 231, 765);
float3 Light3Color = float3(82.5, 0, 0);

float3 Gain = float3(1, 1, 1);



// worldPosition - The vertex position in the world
// vNormal - The normal in the model
float4 ComputeColor(float4 worldPosition, float3 vNormal)
{
    float3 color = LightAmbient;

    float3 normal = normalize(mul(vNormal, World));

    float3 L1 = normalize(Light1Location - worldPosition);
    color += saturate(dot(L1, normal)) * Light1Color;
    
    float3 L2 = Light2Location - worldPosition;
    float L2distance = length(L2);
    L2 /= L2distance;
    color += saturate(dot(L2, normal)) / L2distance * Light2Color;

    float3 L3 = Light3Location - worldPosition;
    float L3distance = length(L3);
    L3 /= L3distance;
    color += saturate(dot(L3, normal)) / L3distance * Light3Color;

    return float4(color, 1);
}
