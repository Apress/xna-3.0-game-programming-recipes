float4x4 xViewProjectionInv;

float xLightStrength;
float3 xLightPosition;
float3 xConeDirection;
float xConeAngle;
float xConeDecay;

Texture xNormalMap;
sampler NormalMapSampler = sampler_state { texture = <xNormalMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xDepthMap;
sampler DepthMapSampler = sampler_state { texture = <xDepthMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexToPixel
{
	float4 Position					: POSITION;
	float2 TexCoord					: TEXCOORD0;
};

struct PixelToFrame
{
	float4 Color			: COLOR0;
};

//------- Technique: DeferredSpotLight --------
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
	
	float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	float depth = tex2D(DepthMapSampler, PSIn.TexCoord).r;
	
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f-1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f-1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	float4 worldPos = mul(screenPos, xViewProjectionInv);
	worldPos /= worldPos.w;			
	
	float3 lightDirection = normalize(worldPos - xLightPosition);		
	float coneDot = dot(lightDirection, normalize(xConeDirection));	
	bool coneCondition = coneDot >= xConeAngle;
	
	float shading = 0;	
	if (coneCondition)
	{
		float coneAttenuation = pow(coneDot, xConeDecay);		
		
		shading = dot(normal, -lightDirection);				
		shading *= xLightStrength;		
		shading *= coneAttenuation;		
	}	
	
	Output.Color.rgb = shading;
	
	return Output;
}

technique DeferredSpotLight
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 MyVertexShader();
        PixelShader  = compile ps_2_0 MyPixelShader();
    }
}
