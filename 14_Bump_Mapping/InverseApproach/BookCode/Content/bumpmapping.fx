float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xLightDirection;
float xTexStretch;

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
Texture xBumpMap;
sampler BumpMapSampler = sampler_state { texture = <xBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

struct BMVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 LightDirT: TEXCOORD1;
};
struct BMPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: BumpMapping --------
BMVertexToPixel BMVertexShader(float4 inPos: POSITION0, float3 inNormal: NORMAL0, float2 inTexCoord: TEXCOORD0, float3 inTangent: TANGENT0)
{
	BMVertexToPixel Output = (BMVertexToPixel)0;	
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);	
	
	Output.TexCoord = inTexCoord;
		
	float3 Binormal = cross(inTangent,inNormal);
	
	float3x3 tangentToObject;
	tangentToObject[0] = normalize(Binormal);
	tangentToObject[1] = normalize(inTangent);
	tangentToObject[2] = normalize(inNormal);		
	
	float3x3 tangentToWorld = mul(tangentToObject, xWorld);
	Output.LightDirT = mul(tangentToWorld, xLightDirection);
	
	return Output;
}

BMPixelToFrame BMPixelShader(BMVertexToPixel PSIn) : COLOR0
{
	BMPixelToFrame Output = (BMPixelToFrame)0;					
		
	float3 bumpColor = tex2D(BumpMapSampler, PSIn.TexCoord*xTexStretch);	
	float3 normalT = (bumpColor - 0.5f)*2.0f;	
	
	float lightFactor = dot(-normalize(normalT), normalize(PSIn.LightDirT));
	float4 texColor = tex2D(TextureSampler, PSIn.TexCoord*xTexStretch);		
	
	Output.Color = lightFactor*texColor;
	
	return Output;
}

technique BumpMapping
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 BMVertexShader();
        PixelShader  = compile ps_2_0 BMPixelShader();
    }
}
