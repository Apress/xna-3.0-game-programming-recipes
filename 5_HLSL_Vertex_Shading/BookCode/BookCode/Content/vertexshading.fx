float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float xAmbient;
float3 xLightDirection;

struct VSVertexToPixel
{
	float4 Position : POSITION;
	float LightFactor	: TEXCOORD0;
};
struct VSPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: VertexShading --------
VSVertexToPixel VSVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0)
{
	VSVertexToPixel Output = (VSVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	float3 normal = normalize(inNormal);
	
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(normal, rotMatrix);
	
	Output.LightFactor = dot(rotNormal, -xLightDirection);
	
	return Output;
}

VSPixelToFrame VSPixelShader(VSVertexToPixel PSIn) : COLOR0
{
	VSPixelToFrame Output = (VSPixelToFrame)0;					
	
	float4 baseColor = float4(0,0,1,1);	
	Output.Color = baseColor*(PSIn.LightFactor+xAmbient);		
	
	return Output;
}

technique VertexShading
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 VSVertexShader();
        PixelShader  = compile ps_2_0 VSPixelShader();
    }
}
