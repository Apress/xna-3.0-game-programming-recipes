float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightPosition;
float xAmbient;

struct PPSVertexToPixel
{
	float4 Position			: POSITION;
	float3 Normal			: TEXCOORD0;
	float3 LightDirection	: TEXCOORD1;
};
struct PPSPixelToFrame
{
	float4 Color 			: COLOR0;
};

//------- Technique: PerPixelShading --------
PPSVertexToPixel PPSVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0)
{
	PPSVertexToPixel Output = (PPSVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	float3 final3DPos = mul(inPos, xWorld);
	Output.LightDirection = final3DPos - xLightPosition;
		
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(inNormal, rotMatrix);
	Output.Normal = rotNormal;
	
	return Output;
}

PPSPixelToFrame PPSPixelShader(PPSVertexToPixel PSIn) : COLOR0
{
	PPSPixelToFrame Output = (PPSPixelToFrame)0;
	
	float4 baseColor = float4(0,0,1,1);		
	
	float3 normal = normalize(PSIn.Normal);	
	float3 lightDirection = normalize(PSIn.LightDirection);	
	float lightFactor = dot(normal, -lightDirection);
	
	Output.Color = baseColor*(lightFactor+xAmbient);		
	
	return Output;
}

technique PerPixelShading
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 PPSVertexShader();
        PixelShader  = compile ps_2_0 PPSPixelShader();
    }
}
