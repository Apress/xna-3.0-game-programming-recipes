float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float xAmbient;
float3 xLightPosition;

struct PLVertexToPixel
{
	float4 Position : POSITION;
	float LightFactor	: TEXCOORD0;
};
struct PLPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: VertexShading --------
PLVertexToPixel PLVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0)
{
	PLVertexToPixel Output = (PLVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	float3 normal = normalize(inNormal);
	
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(normal, rotMatrix);
	
	float3 final3DPosition = mul(inPos, xWorld);
	float3 lightDirection = final3DPosition - xLightPosition;
	float distance = length(lightDirection);
	lightDirection = normalize(lightDirection);
	
	Output.LightFactor = dot(rotNormal, -lightDirection);
	Output.LightFactor /= distance;
	
	return Output;
}

PLPixelToFrame PLPixelShader(PLVertexToPixel PSIn) : COLOR0
{
	PLPixelToFrame Output = (PLPixelToFrame)0;					
	
	float4 baseColor = float4(0,0,1,1);	
	Output.Color = baseColor*(PSIn.LightFactor+xAmbient);		
	
	return Output;
}

technique PointLight
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 PLVertexShader();
        PixelShader  = compile ps_2_0 PLPixelShader();
    }
}
