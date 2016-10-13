float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightPosition;
float3 xCameraPos;
float xAmbient;
float xSpecularPower;
float xLightStrength;

struct SLVertexToPixel
{
	float4 Position			: POSITION;
	float3 Normal			: TEXCOORD0;
	float3 LightDirection	: TEXCOORD1;
	float3 EyeDirection		: TEXCOORD2;
};
struct SLPixelToFrame
{
	float4 Color 			: COLOR0;
};

//------- Technique: PerPixelShading --------
SLVertexToPixel SLVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0)
{
	SLVertexToPixel Output = (SLVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	float3 final3DPos = mul(inPos, xWorld);
	Output.LightDirection = final3DPos - xLightPosition;
	Output.EyeDirection = final3DPos - xCameraPos;
		
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(inNormal, rotMatrix);
	Output.Normal = rotNormal;
	
	return Output;
}

SLPixelToFrame SLPixelShader(SLVertexToPixel PSIn) : COLOR0
{
	SLPixelToFrame Output = (SLPixelToFrame)0;
	
	float4 baseColor = float4(0,0,1,1);		
	
	float3 normal = normalize(PSIn.Normal);	
	float3 lightDirection = normalize(PSIn.LightDirection);	
	float shading = dot(normal, -lightDirection);
	shading *= xLightStrength;
	
	float3 reflection = -reflect(lightDirection, normal);
	float3 eyeDirection = normalize(PSIn.EyeDirection);
	float specular = dot(reflection, eyeDirection);
	specular = pow(specular, xSpecularPower);	
	specular *= xLightStrength;
	
	Output.Color = baseColor*(shading+xAmbient)+specular;		
	
	return Output;
}

technique SpecularLighting
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 SLVertexShader();
        PixelShader  = compile ps_2_0 SLPixelShader();
    }
}
