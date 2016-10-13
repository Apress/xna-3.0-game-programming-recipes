float xAmbient;

Texture xColorMap;
sampler ColorMapSampler = sampler_state { texture = <xColorMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xShadingMap;
sampler ShadingMapSampler = sampler_state { texture = <xShadingMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexToPixel
{
	float4 Position			: POSITION;
	float2 TexCoord			: TEXCOORD0;
};

struct PixelToFrame
{
	float4 Color			: COLOR0;
};

//------- Technique: CombineColorAndShading --------
VertexToPixel MyVertexShader(float4 inPos: POSITION0, float2 texCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;	
	Output.Position = inPos;	
	Output.TexCoord = texCoord;
	return Output;
}

PixelToFrame MyPixelShader(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	float4 color = tex2D(ColorMapSampler, PSIn.TexCoord);
	float shading = tex2D(ShadingMapSampler, PSIn.TexCoord);	
	
	Output.Color = color*(xAmbient + shading);
	
	return Output;
}

technique CombineColorAndShading
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 MyVertexShader();
        PixelShader  = compile ps_2_0 MyPixelShader();
    }
}