float4x4 xViewProjectionInv;
float4x4 xLightViewProjection;

float xLightStrength;
float3 xLightPosition;
float3 xConeDirection;
float xConeAngle;
float xConeDecay;

Texture xNormalMap;
sampler NormalMapSampler = sampler_state { texture = <xNormalMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xDepthMap;
sampler DepthMapSampler = sampler_state { texture = <xDepthMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xShadowMap;
sampler ShadowMapSampler = sampler_state { texture = <xShadowMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xPreviousShadingContents;
sampler PreviousSampler = sampler_state { texture = <xPreviousShadingContents> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

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
	
	//sample normal from normal map
	float3 normal = tex2D(NormalMapSampler, PSIn.TexCoord).rgb;
	normal = normal*2.0f-1.0f;
	normal = normalize(normal);
	
	//sample depth from depth map
	float depth = tex2D(DepthMapSampler, PSIn.TexCoord).r;
	
	//create screen position
	float4 screenPos;
	screenPos.x = PSIn.TexCoord.x*2.0f-1.0f;
	screenPos.y = -(PSIn.TexCoord.y*2.0f-1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;	
	
	//transform to 3D position
	float4 worldPos = mul(screenPos, xViewProjectionInv);
	worldPos /= worldPos.w;		
	
	//find screen position as seen by the light
	float4 lightScreenPos = mul(worldPos, xLightViewProjection);
	lightScreenPos /= lightScreenPos.w;
	
	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x/2.0f+0.5f;
	lightSamplePos.y = (-lightScreenPos.y/2.0f+0.5f);
	
	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;	
	float distanceStoredInDepthMap = tex2D(ShadowMapSampler, lightSamplePos);	
	bool shadowCondition =  distanceStoredInDepthMap <= realDistanceToLight - 1.0f/100.0f;	
	
	//determine cone criteria
	float3 lightDirection = normalize(worldPos - xLightPosition);		
	float coneDot = dot(lightDirection, normalize(xConeDirection));	
	bool coneCondition = coneDot >= xConeAngle;
	
	//calculate shading
	float shading = 0;	
	if (coneCondition && !shadowCondition)
	{
		float coneAttenuation = pow(coneDot, xConeDecay);			
		shading = dot(normal, -lightDirection);				
		shading *= xLightStrength;		
		shading *= coneAttenuation;		
	}	
	
	float4 previous = tex2D(PreviousSampler, PSIn.TexCoord);
	Output.Color = previous + shading;
	
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
