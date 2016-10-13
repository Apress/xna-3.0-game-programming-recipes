float xTime;
float xBlurSize = 0.5f;

texture textureToSampleFrom;
sampler textureSampler = sampler_state
{
	texture = <textureToSampleFrom>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
};

texture originalImage;
sampler originalSampler = sampler_state
{
	texture = <originalImage>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
};

float positions[] = 
{
	0.0f,
	0.005,	
	0.01166667,	
	0.01833333,	
	0.025,	
	0.03166667,	
	0.03833333,	
	0.045,	
};

float weights[] =
{
	0.0530577,
	0.1028506,
	0.09364651,
	0.0801001,
	0.06436224,
	0.04858317,
	0.03445063,
	0.02294906,
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

//------- PP Technique: HorBlur --------
PPPixelToFrame HorBlurPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;			
    
    for (int i = 0; i < 8; i++)
    {
		float4 samplePos = tex2D(textureSampler, PSIn.TexCoord + float2(positions[i], 0)*xBlurSize);
		samplePos *= weights[i];
		float4 sampleNeg = tex2D(textureSampler, PSIn.TexCoord - float2(positions[i], 0)*xBlurSize);
		sampleNeg *= weights[i];
        Output.Color += samplePos + sampleNeg;
    }

	return Output;
}

technique HorBlur
{
	pass Pass0
    {  
    	VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader  = compile ps_2_0 HorBlurPS();
    }
}

//------- PP Technique: VerBlur --------
PPPixelToFrame VerBlurPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;			
    
    for (int i = 0; i < 8; i++)
    {
		float4 samplePos = tex2D(textureSampler, PSIn.TexCoord + float2(0, positions[i])*xBlurSize);
		samplePos *= weights[i];
		float4 sampleNeg = tex2D(textureSampler, PSIn.TexCoord - float2(0, positions[i])*xBlurSize);
		sampleNeg *= weights[i];
        Output.Color += samplePos + sampleNeg;
    }

	return Output;
}

technique VerBlur
{
	pass Pass0
    {  
    	VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader  = compile ps_2_0 VerBlurPS();
    }
}

//------- PP Technique: VerBlurAndGlow --------
PPPixelToFrame BlendInPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;		
	
	float4 finalColor = tex2D(originalSampler, PSIn.TexCoord);
	finalColor.a = 0.3f;
	
	Output.Color = finalColor;
	return Output;
}


technique VerBlurAndGlow
{
	pass Pass0
    {  
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader  = compile ps_2_0 VerBlurPS();
    }
    pass Pass1
    {  
    	AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        PixelShader  = compile ps_2_0 BlendInPS();
    }
}
