float xTime;

texture textureToSampleFrom;
sampler textureSampler = sampler_state
{
	texture = <textureToSampleFrom>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
};

struct PPVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord	: TEXCOORD0;
};
struct PPPixelToFrame
{
    float4 Color 	: COLOR0;
};

PPVertexToPixel PassThroughVertexShader(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	PPVertexToPixel Output = (PPVertexToPixel)0;
	Output.Position = inPos;
	Output.TexCoord = inTexCoord;
	return Output;
}

//------- PP Technique: Invert --------
PPPixelToFrame InvertPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;		
	
	float4 colorFromTexture = tex2D(textureSampler, PSIn.TexCoord);
	Output.Color = 1-colorFromTexture;

	return Output;
}

technique Invert
{
	pass Pass0
    {  
    	VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader  = compile ps_1_1 InvertPS();
    }
}

//------- PP Technique: TimeChange --------
PPPixelToFrame TimeChangePS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;		
	
	Output.Color = tex2D(textureSampler, PSIn.TexCoord);
	Output.Color.b *= sin(xTime);
	Output.Color.rg *= cos(xTime);
	Output.Color += 0.2f;

	return Output;
}

technique TimeChange
{
	pass Pass0
    {  
    	VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader  = compile ps_2_0 TimeChangePS();
    }
}

