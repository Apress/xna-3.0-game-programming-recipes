float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float xTime;

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct CVVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Extra	: TEXCOORD1;
};
struct CVPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: CustomVertexShader --------
CVVertexToPixel CVVertexShader(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0, float4 inExtra: TEXCOORD1)
{
	CVVertexToPixel Output = (CVVertexToPixel)0;
	
	float4 origPos = float4(inPos, 1);
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(origPos, preWorldViewProjection);
	
	Output.Extra = sin(xTime*inExtra.xyz);
	
	Output.TexCoord = inTexCoord;
	Output.TexCoord.x += sin(xTime)*inExtra.w;
	Output.TexCoord.y -= sin(xTime)*inExtra.w;
	
	return Output;
}

CVPixelToFrame CVPixelShader(CVVertexToPixel PSIn) : COLOR0
{
	CVPixelToFrame Output = (CVPixelToFrame)0;			
	
	Output.Color = tex2D(TextureSampler, PSIn.TexCoord);
	
	Output.Color.rgb += PSIn.Extra.rgb;	
	
	return Output;
}

technique CustomVertexShader
{
	pass Pass0
    {  
    	VertexShader = compile vs_1_1 CVVertexShader();
        PixelShader  = compile ps_2_0 CVPixelShader();
    }
}
