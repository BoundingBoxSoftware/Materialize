// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#include "UnityPBSLighting.cginc"
#include "UnityMetaPass.cginc"

float _FlipNormals;

int _IsMover;
float4x4 _CURRENT_VP_MATRIX;
float4x4 _LAST_VP_MATRIX;
float4x4 _LAST_MODEL_MATRIX;
float2 _JitterSample;

float rand(float2 co){
    return frac( sin( dot( co.xy ,float2(12.9898,78.233) ) ) * 43758.5453);
}

float rand(float3 co){
    return frac( sin( dot( co.xyz ,float3(12.9898,78.233, 137.9462) ) ) * 43758.5453);
}

float randScreen( float2 co){
	co = floor( co * _ScreenParams );
	return frac( sin( dot( co.xy ,float2(12.9898,78.233) ) ) * 43758.5453);
}


#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

//==================================================================//
//						Standard surface stuffs						//
//==================================================================//



// vertex-to-fragment interpolation data
struct v2f_surf {
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD0; // _MainTex
	float4 tSpace0 : TEXCOORD1;
	float4 tSpace1 : TEXCOORD2;
	float4 tSpace2 : TEXCOORD3;
	
	// Motion Blur
	float4 screenPos : TEXCOORD4;
	float4 lastScreenPos : TEXCOORD5;

	// Enlighten Global Illumination
	float4 lmap : TEXCOORD6;
	#ifdef LIGHTMAP_OFF
	  #if UNITY_SHOULD_SAMPLE_SH
	    half3 sh : TEXCOORD7; // SH
	  #endif
	#else
	  #ifdef DIRLIGHTMAP_OFF
	    float4 lmapFadePos : TEXCOORD8;
	  #endif
	#endif

	//#ifdef TRIPLANAR
		// Y projection
		float4 tSpaceY : TEXCOORD9;
		float4 bSpaceY : TEXCOORD10;
		float4 localNormal : TEXCOORD11;
	//#endif

};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
	v2f_surf o;
	UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
	/*
	float4x4 JITTER_MATRIX_P = UNITY_MATRIX_P;
	JITTER_MATRIX_P[0][2] += ( _JitterSample.x * 2.0 - 1.0 ) / _ScreenParams.x;
	JITTER_MATRIX_P[1][2] += ( _JitterSample.y * 2.0 - 1.0 ) / _ScreenParams.y;
	o.pos = mul( JITTER_MATRIX_P, mul ( UNITY_MATRIX_MV, v.vertex ) );
	*/
	o.pos = UnityObjectToClipPos ( v.vertex );
	 
	o.uv = v.texcoord.xy;
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
	o.color = v.color;

	//#ifdef TRIPLANAR
		// Y projection
		fixed3 worldTangentY = float3(1,0,0);
		if( abs( v.normal.z ) != 1 ){
			worldTangentY = normalize( cross(v.normal.xyz, float3(0,0,-1) ) );
		}
		worldTangentY = normalize( mul( unity_ObjectToWorld, float4( worldTangentY, 0 ) ).xyz );
		o.tSpaceY = float4( worldTangentY, v.vertex.x );
		o.bSpaceY = float4( normalize( cross( worldNormal, worldTangentY ) ), v.vertex.y );
		o.localNormal = float4( v.normal.xyz, v.vertex.z );
	//#endif
  	
  	// Motion Blur
  	//float4 screenPos = ComputeScreenPos( o.pos );
  	float4 screenPos = ComputeScreenPos( mul ( _CURRENT_VP_MATRIX, float4( worldPos, 1 ) ) );		
	float4 lastScreenPos;
	if( _IsMover == 1 ){
		float3 lastWorldPosition = mul ( _LAST_MODEL_MATRIX, v.vertex ).xyz;
		lastScreenPos = ComputeScreenPos( mul ( _LAST_VP_MATRIX, float4( lastWorldPosition, 1 ) ) );
	}else{
		lastScreenPos = ComputeScreenPos( mul ( _LAST_VP_MATRIX, float4( worldPos, 1 ) ) );	
	}
	o.screenPos = screenPos;
	o.lastScreenPos = lastScreenPos;

	// Enlighten Global Illumination
	#ifndef DYNAMICLIGHTMAP_OFF
	o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#else
	  o.lmap.zw = 0;
	#endif
	#ifndef LIGHTMAP_OFF
	  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	  #ifdef DIRLIGHTMAP_OFF
	    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
	    o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
	  #endif
	#else
	  o.lmap.xy = 0;
	  #if UNITY_SHOULD_SAMPLE_SH
	    #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
	      o.sh = 0;
	    #elif (SHADER_TARGET < 30)
	      o.sh = ShadeSH9 (float4(worldNormal,1.0));
	    #else
	      o.sh = ShadeSH3Order (half4(worldNormal, 1.0));
	    #endif
	  #endif
	#endif

	return o;
}


//==================================================================//
//						Meta surface stuffs							//
//==================================================================//



struct VertexInputMeta
{
	float4 vertex	: POSITION;
	half3 normal	: NORMAL;
	float2 uv0		: TEXCOORD0;
	float2 uv1		: TEXCOORD1;
//#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	float2 uv2		: TEXCOORD2;
//#endif
//#ifdef _TANGENT_TO_WORLD
	half4 tangent	: TANGENT;
//#endif
	float4 color	: COLOR;
};

struct v2f_meta
{
	float4 uv		: TEXCOORD0;
	float4 pos		: SV_POSITION;
	fixed4 color	: COLOR;
	float4 tSpace0 : TEXCOORD1;
	float4 tSpace1 : TEXCOORD2;
	float4 tSpace2 : TEXCOORD3;
	
};

v2f_meta vert_meta (VertexInputMeta v)
{
	v2f_meta o;
	o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	
	o.uv.xy = v.uv0.xy;
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
	o.color = v.color;
	
	
	return o;
}

float3 unifyNormals( float4 texNormal ) {
	texNormal.xy = texNormal.wy * 2.0 - 1.0;
	texNormal.y *= -1.0;
	texNormal.z = sqrt( 1.0 - saturate( dot( texNormal.xy, texNormal.xy ) ) );
	texNormal.xyz = normalize( texNormal.xyz );
	//When scaling an object negatively the y channel needs to be flipped
	texNormal.y *= ( 1.0 - _FlipNormals ) * 2.0 - 1.0;
	return texNormal.xyz;
}

float3 unifyNormalsNorm( float3 texNormal ) {
	texNormal.y *= -1.0;
	texNormal.z = sqrt( 1.0 - saturate( dot( texNormal.xy, texNormal.xy ) ) );
	texNormal.xyz = normalize( texNormal.xyz );
	//When scaling an object negatively the y channel needs to be flipped
	texNormal.y *= ( 1.0 - _FlipNormals ) * 2.0 - 1.0;
	return texNormal.xyz;
}

float2 CalcMotionVector( v2f_surf IN ){
	float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
	screenPos.y = 1.0 - screenPos.y;
	float2 lastScreenPos = IN.lastScreenPos.xy / IN.lastScreenPos.w;
	lastScreenPos.y = 1.0 - lastScreenPos.y;
	float2 motionVector = ( screenPos - lastScreenPos );
	float2 mvStep = step( float2(0,0), motionVector );
	//One Over Method
	motionVector = 1.0 / ( abs( motionVector * _ScreenParams.xy ) + 1.0 );
	//Scaled Method
	//motionVector = pow( abs( motionVector * _ScreenParams.xy * ( 1.0/64.0 ) ), 0.5 );
	motionVector = lerp( -motionVector, motionVector, mvStep ) * 0.5 + 0.5;
	
	
	return motionVector;
} 


void ReturnOutput ( SurfaceOutputStandard surfOut, float3 worldPos, float3 worldViewDir, v2f_surf IN, out half4 outDiffuse, out half4 outMST, out half4 outNormal, out half4 outEmission ) {
	half atten = 1;
				
	UnityGI gi;
	UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
	gi.indirect.diffuse = 0;
	gi.indirect.specular = 0;
	gi.light.color = 0;
	gi.light.dir = half3(0,1,0);
	gi.light.ndotl = LambertTerm (surfOut.Normal, gi.light.dir);
	// Call GI (lightmaps/SH/reflections) lighting function
	UnityGIInput giInput;
	UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
	giInput.light = gi.light;
	giInput.worldPos = worldPos;
	giInput.worldViewDir = worldViewDir;
	giInput.atten = atten;
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	    giInput.lightmapUV = IN.lmap;
	#else
	    giInput.lightmapUV = 0.0;
	#endif
	#if UNITY_SHOULD_SAMPLE_SH
	    giInput.ambient = IN.sh;
	#else
	    giInput.ambient.rgb = 0.0;
	#endif
	giInput.probeHDR[0] = unity_SpecCube0_HDR;
	giInput.probeHDR[1] = unity_SpecCube1_HDR;
	#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
	    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
	#endif
	#if UNITY_SPECCUBE_BOX_PROJECTION
	    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
	#endif
	LightingStandard_GI(surfOut, giInput, gi);
	
	// call lighting function to output g-buffer
	outEmission = LightingStandard_Deferred (surfOut, worldViewDir, gi, outDiffuse, outMST, outNormal);
	
	#ifndef UNITY_HDR_ON
	outEmission.rgb = exp2(-outEmission.rgb);
	#endif
	
}