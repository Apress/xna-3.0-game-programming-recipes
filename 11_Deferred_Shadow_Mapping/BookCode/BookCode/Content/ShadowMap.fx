float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
Texture xTexture;

struct VertexToPixel
{
	float4 Position			: POSITION;
	float4 ScreenPos		: TEXCOORD1;
};
struct PixelToFrame
{
	float4 Color 			: COLOR0;
};

//------- Technique: ShadowMap --------
VertexToPixel MyVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0)
{
	VertexToPixel Output = (VertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);		
	
	Output.ScreenPos = Output.Position;
	
	return Output;
}

PixelToFrame MyPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.ScreenPos.z/PSIn.ScreenPos.w;	
	
	return Output;
}

technique ShadowMap
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 MyVertexShader();
        PixelShader  = compile ps_2_0 MyPixelShader();
    }
}
