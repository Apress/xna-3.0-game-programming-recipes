//------- XNA interface --------
float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float4x4 xMirrorView;

//------- Texture Samplers --------
Texture xMirrorTexture;
sampler textureSampler = sampler_state { texture = <xMirrorTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = WRAP; AddressV = WRAP;};

struct MirVertexToPixel
{
	float4 Position : POSITION;
	float4 TexCoord	: TEXCOORD0;
};
struct MirPixelToFrame
{
    float4 Color 	: COLOR0;
};

//------- Technique: Mirror --------
MirVertexToPixel MirrorVS(float4 inPos: POSITION0)
{
	MirVertexToPixel Output = (MirVertexToPixel)0;		
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);
	
	float4x4 preMirrorViewProjection = mul (xMirrorView, xProjection);
	float4x4 preMirrorWorldViewProjection = mul(xWorld, preMirrorViewProjection);
	Output.TexCoord = mul(inPos, preMirrorWorldViewProjection);	
	
	return Output;
}

MirPixelToFrame MirrorPS(MirVertexToPixel PSIn) : COLOR0
{
	MirPixelToFrame Output = (MirPixelToFrame)0;		
	
	float2 ProjectedTexCoords;
    ProjectedTexCoords[0] = PSIn.TexCoord.x/PSIn.TexCoord.w/2.0f +0.5f;
    ProjectedTexCoords[1] = -PSIn.TexCoord.y/PSIn.TexCoord.w/2.0f +0.5f;
    Output.Color = tex2D(textureSampler, ProjectedTexCoords);
	
	return Output;
}

technique Mirror
{
	pass Pass0
    {          
    	VertexShader = compile vs_1_1 MirrorVS();
        PixelShader  = compile ps_2_0 MirrorPS();        
    }
}
