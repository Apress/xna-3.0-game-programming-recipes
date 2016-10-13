float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

float4 xWaveSpeeds;
float4 xWaveHeights;
float4 xWaveLengths;
float2 xWaveDir0;
float2 xWaveDir1;
float2 xWaveDir2;
float2 xWaveDir3;

float3 xCameraPos;
float xBumpStrength;
float xTexStretch;
float xTime;

Texture xCubeMap;
samplerCUBE CubeMapSampler = sampler_state { texture = <xCubeMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xBumpMap;
sampler BumpMapSampler = sampler_state { texture = <xBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct OWVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Pos3D	: TEXCOORD1;
	float3x3 TTW	: TEXCOORD2;	
};
struct OWPixelToFrame
{
    float4 Color 	: COLOR0;
};
    
//------- Technique: OceanWater --------
OWVertexToPixel OWVertexShader(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	OWVertexToPixel Output = (OWVertexToPixel)0;	
	
	float4 dotProducts;
    dotProducts.x = dot(xWaveDir0, inPos.xz);
    dotProducts.y = dot(xWaveDir1, inPos.xz);
    dotProducts.z = dot(xWaveDir2, inPos.xz);
    dotProducts.w = dot(xWaveDir3, inPos.xz);        
    
    float4 arguments = dotProducts/xWaveLengths+xTime*xWaveSpeeds;
    float4 heights = xWaveHeights*sin(arguments);
    
    float4 final3DPos = inPos;
    final3DPos.y += heights.x;
    final3DPos.y += heights.y;
    final3DPos.y += heights.z;
    final3DPos.y += heights.w;
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(final3DPos, preWorldViewProjection);	
	
	float4 final3DPosW = mul(final3DPos, xWorld);
	Output.Pos3D = final3DPosW;	
	
	float4 derivatives = xWaveHeights*cos(arguments)/xWaveLengths;
	float2 deviations = 0;
    deviations += derivatives.x*xWaveDir0;
    deviations += derivatives.y*xWaveDir1;
    deviations += derivatives.z*xWaveDir2;
    deviations += derivatives.w*xWaveDir3;	
	
	float3 Normal = float3(-deviations.x, 1, -deviations.y);	
	float3 Binormal = float3(1, deviations.x, 0);
    float3 Tangent = float3(0, deviations.y, 1);    
	
	float3x3 tangentToObject;
	tangentToObject[0] = normalize(Binormal);
	tangentToObject[1] = normalize(Tangent);
	tangentToObject[2] = normalize(Normal);		
	
	float3x3 tangentToWorld = mul(tangentToObject, xWorld);
	Output.TTW = tangentToWorld;		
	
	Output.TexCoord = inTexCoord+xTime/50.0f*float2(-1,0);
	
	return Output;
}

OWPixelToFrame OWPixelShader(OWVertexToPixel PSIn) : COLOR0
{
	OWPixelToFrame Output = (OWPixelToFrame)0;					
		
	float3 bumpColor1 = tex2D(BumpMapSampler, xTexStretch*PSIn.TexCoord)-0.5f;	
	float3 bumpColor2 = tex2D(BumpMapSampler, xTexStretch*1.8*PSIn.TexCoord.yx)-0.5f;
	float3 bumpColor3 = tex2D(BumpMapSampler, xTexStretch*3.1*PSIn.TexCoord)-0.5f;
	
	float3 normalT = bumpColor1 + bumpColor2 + bumpColor3;
	
	normalT.rg *= xBumpStrength;
	normalT = normalize(normalT);
	float3 normalW = mul(normalT, PSIn.TTW);	
	
	float3 eyeVector = normalize(PSIn.Pos3D - xCameraPos);	
	float3 reflection = reflect(eyeVector, normalW);		
	float4 reflectiveColor = texCUBE(CubeMapSampler, reflection);
	
	float fresnelTerm = dot(-eyeVector, normalW);	
	fresnelTerm = fresnelTerm/2.0f+0.5f;		
	
	float sunlight = reflectiveColor.r;
	sunlight += reflectiveColor.g;
	sunlight += reflectiveColor.b;
	sunlight /= 3.0f;
	float specular = pow(sunlight,30);	
	
	float4 waterColor = float4(0,0.15,0.4,1);	
	Output.Color = waterColor*fresnelTerm + reflectiveColor*(1-fresnelTerm) + specular;	
		
	return Output;
}

technique OceanWater
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 OWVertexShader();
        PixelShader  = compile ps_2_0 OWPixelShader();
    }
}
