float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

struct VertexToPixel
{
	float4 Position			: POSITION;
	float3 Normal			: TEXCOORD0;
	float4 ScreenPos		: TEXCOORD1;
	float2 TexCoords		: TEXCOORD2;
};
struct PixelToFrame
{
	float4 Color 			: COLOR0;
	float4 Normal 			: COLOR1;
	float4 Depth 			: COLOR2;
};

//------- Technique: MultipleTargets --------
VertexToPixel MyVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0, float2 inTexCoords: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);		
		
	float3x3 rotMatrix = (float3x3)xWorld;
	float3 rotNormal = mul(inNormal, rotMatrix);
	Output.Normal = rotNormal;
	
	Output.ScreenPos = Output.Position;	
	Output.TexCoords = inTexCoords;
	
	return Output;
}

PixelToFrame MyPixelShader(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;	
	
	Output.Color.rgb = tex2D(TextureSampler, PSIn.TexCoords);
	Output.Normal.xyz = PSIn.Normal/2.0f+0.5f;
	
	Output.Depth = PSIn.ScreenPos.z/PSIn.ScreenPos.w;	
	
	return Output;
}

technique MultipleTargets
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 MyVertexShader();
        PixelShader  = compile ps_2_0 MyPixelShader();
    }
}
