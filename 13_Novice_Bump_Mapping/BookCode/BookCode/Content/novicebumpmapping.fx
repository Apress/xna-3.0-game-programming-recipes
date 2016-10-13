float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
Texture xBumpMap;
sampler BumpMapSampler = sampler_state { texture = <xBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

struct SBMVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};
struct SBMPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: BumpMapping --------
SBMVertexToPixel SBMVertexShader(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	SBMVertexToPixel Output = (SBMVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	Output.TexCoord = inTexCoord;
	
	return Output;
}

SBMPixelToFrame SBMPixelShader(SBMVertexToPixel PSIn) : COLOR0
{
	SBMPixelToFrame Output = (SBMPixelToFrame)0;					
	
	float3 bumpMapColor = tex2D(BumpMapSampler, PSIn.TexCoord).rbg;
	float3 normalFromBumpMap = (bumpMapColor - 0.5f)*2.0f;
	
	float lightFactor = dot(-normalize(normalFromBumpMap), normalize(xLightDirection));
	float4 texColor = tex2D(TextureSampler, PSIn.TexCoord);	
	
	Output.Color = lightFactor*texColor;		
	
	return Output;
}

technique SimpleBumpMapping
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 SBMVertexShader();
        PixelShader  = compile ps_2_0 SBMPixelShader();
    }
}
