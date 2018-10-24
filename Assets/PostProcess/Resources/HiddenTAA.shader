// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TAA" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	

		
	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	sampler2D _DofPrepass;
	sampler2D _AccumulatedFrames;
	sampler2D _AccumulatedDofPrepass;
	sampler2D _BlendAmount;
	sampler2D _MotionVector;
	
	sampler2D _CameraGBufferTexture0;	// Diffuse color (RGB), unused (A)
	sampler2D _CameraGBufferTexture1;	// Specular color (RGB), roughness (A)
	sampler2D _CameraGBufferTexture2;	// World space normal (RGB), unused (A)
	sampler2D _CameraGBufferTexture3;	// ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer
	uniform float4 _CameraGBufferTexture0_TexelSize;
	uniform float4 _CameraGBufferTexture1_TexelSize;
	uniform float4 _CameraGBufferTexture2_TexelSize;
	uniform float4 _CameraGBufferTexture3_TexelSize;
	
	float4x4 _INVERSE_VP_MATRIX;
	float4x4 _LAST_VP_MATRIX;
	float4x4 _LAST_BG_VP_MATRIX;
	
	sampler2D_float _CameraDepthTexture;
	uniform float4 _CameraDepthTexture_TexelSize;
	
 	float2 _RenderBufferSize;
 	
 	float _FogDensity;
 	float4 _FogColor;
 	
 	float _FocalDepth;
	float _DOFMaxDistance;

	float _OverideBlend;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float4 screenPos : TEXCOORD1;
		float4 lastScreenPos : TEXCOORD2;
		float3 cameraRay : TEXCOORD3;
	};

	v2f vert(appdata_img v)
	{
	   	v2f o;
		o.pos = UnityObjectToClipPos( v.vertex);
		o.uv = v.texcoord;
		
		float4 screenPos = float4( v.texcoord.xy * 2 - 1, 1, 1 );
		float4 worldPos = mul( _INVERSE_VP_MATRIX, screenPos );
		worldPos = worldPos / worldPos.w;
		float4 lastScreenPos = mul( _LAST_VP_MATRIX, float4( worldPos.xyz, 1 ) );
		
		o.screenPos = ComputeScreenPos( screenPos );
		o.lastScreenPos = ComputeScreenPos( lastScreenPos );
		
		float4 cameraRay = float4( v.texcoord * 2.0 - 1.0, 1.0, 1.0);
		cameraRay = mul( _INVERSE_VP_MATRIX, cameraRay);
		o.cameraRay = cameraRay / cameraRay.w;
		
	   	return o;
	}
	
	static const int SmallKernelSamples = 4;		
	static const float3 SmallKernel[SmallKernelSamples] =
	{
		float3(0.5,0.5,1),
		float3(-0.5,0.5,1),
		float3(0.5,-0.5,1),
		float3(-0.5,-0.5,1)
	};
	
	static const int BlurKernelSamples = 9;		
	static const float3 BlurKernel[BlurKernelSamples] =
	{
		float3(1,1,0.5),
		float3(0,1,0.7),
		float3(-1,1,0.5),
		float3(1,0,0.7),
		float3(0,0,1.0),
		float3(-1,0,0.7),
		float3(1,-1,0.5),
		float3(0,-1,0.7),
		float3(-1,-1,0.5)
	};
	
	static const int BlurKernel2Samples = 5;		
	static const float3 BlurKernel2[BlurKernel2Samples] =
	{
		float3(0,1,0.7),
		float3(1,0,0.7),
		float3(0,0,1.0),
		float3(-1,0,0.7),
		float3(0,-1,0.7)
	};

	half2 sampleMV (float2 ScreenUV) {
		half2 motionVector = half2( tex2Dlod(_CameraGBufferTexture0, float4(ScreenUV,0,0) ).w, tex2Dlod( _CameraGBufferTexture1, float4(ScreenUV,0,0) ).w ) * 2.0 - 1.0;		
		half2 mvStep = step( float2(0,0), motionVector );
		// One over method
		motionVector = ( ( 1.0 / abs( motionVector ) ) - 1.0 );
		// scaled method
		//motionVector = ( motionVector * motionVector * 64.0 );
		return lerp( -motionVector, motionVector, mvStep );
	}

	half2 dialateMV (float2 ScreenUV, float2 OneOverRTBSize){
		half2 motionVector = float2(0,0);
		half maxLength = 0;
		half2 sampleMotionVector = float2(0,0);
		half sampleLength = 0;
		for( int i = 0; i < BlurKernelSamples; i++ ){
			sampleMotionVector = sampleMV( ScreenUV + BlurKernel[i].xy * OneOverRTBSize );
			sampleLength = length(sampleMotionVector);
			if( sampleLength > maxLength ){
				maxLength = sampleLength;
				motionVector = sampleMotionVector;
			}
		}
		return motionVector;
	}
	
	half4 fragMotion (v2f IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float2 OneOverRTBSize = _CameraGBufferTexture0_TexelSize;//( 1.0 / _RenderBufferSize.xy );
		
		half coverage = tex2Dlod( _CameraGBufferTexture2, float4(ScreenUV,0,0) ).w;
		
		half4 MST = tex2Dlod( _CameraGBufferTexture1, float4(ScreenUV,0,0) );

		half2 motionVector = float2(0,0);

		if( coverage == 0 ){
			float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
			screenPos.y = 1.0 - screenPos.y;
			float2 lastScreenPos = IN.lastScreenPos.xy / IN.lastScreenPos.w;
			lastScreenPos.y = 1.0 - lastScreenPos.y;
			motionVector = ( screenPos - lastScreenPos );
			motionVector = motionVector * _RenderBufferSize.xy;
		}else{
			motionVector = dialateMV(ScreenUV, OneOverRTBSize);
		}
		
		return half4( motionVector, 0, 0 );
		
	}
	
	half4 fragBlend (v2f IN) : COLOR
	{
	
		float2 ScreenUV = IN.uv;
		float2 OneOverRTBSize = ( 1.0 / _RenderBufferSize.xy );

		float2 motionVector = tex2Dlod( _MotionVector, float4(ScreenUV,0,0) ).xy;
		
		half2 LastScreenUV = ScreenUV - ( motionVector * OneOverRTBSize );
		
		// Color Diff
		half3 thisColor = saturate( tex2D( _AccumulatedFrames, LastScreenUV ).xyz );
		half3 sampleMin = half3(1,1,1);
		half3 sampleMax = half3(0,0,0);
		for( int i = 0; i < BlurKernelSamples; i++ ){
			half3 sampleColor = saturate( tex2D( _MainTex, ScreenUV + BlurKernel[i].xy * OneOverRTBSize ).xyz );
			sampleMin = min( sampleColor, sampleMin );
			sampleMax = max( sampleColor, sampleMax );
		}
		half3 blendAmountColor = smoothstep( sampleMin - 0.01, sampleMin, thisColor );
		blendAmountColor *= 1.0 - smoothstep( sampleMax, sampleMax + 0.01, thisColor );

		//half3 blendAmountColor = smoothstep( sampleMin - 0.01, sampleMin + 0.01, thisColor );
		//blendAmountColor *= 1.0 - smoothstep( sampleMax - 0.01, sampleMax + 0.01, thisColor );
		
		// DOF Diff
		//half thisDOF = tex2D( _AccumulatedDofPrepass, LastScreenUV ).x;
		//half sampleDOFMin = 1;
		//half sampleDOFMax = 0;
		//for( int i = 0; i < BlurKernelSamples; i++ ){
		//	half sampleDOF = tex2D( _DofPrepass, ScreenUV + BlurKernel[i].xy * OneOverRTBSize ).x;
		//	sampleDOFMin = min( sampleDOF, sampleDOFMin );
		//	sampleDOFMax = max( sampleDOF, sampleDOFMax );
		//}
		//half blendAmountDOF = smoothstep( sampleDOFMin - 0.1, sampleDOFMin, thisDOF );
		//blendAmountDOF *= 1.0 - smoothstep( sampleDOFMax, sampleDOFMax + 0.1, thisDOF );
		
		half blendAmount = min( min( blendAmountColor.x, blendAmountColor.y ), blendAmountColor.z );
		//blendAmount *= blendAmountDOF;
		
		blendAmount *= 1.0 - smoothstep( 32, 64, length( motionVector ) );

		if (_OverideBlend > 0.5) {
			blendAmount = 1.0;
		}

		blendAmount *= lerp( 1.0, tex2D( _BlendAmount, ScreenUV ).x, 0.75 );
		blendAmount = min( blendAmount, 0.95 );
		
		return float4( blendAmount.xxx, 1 );
	}

	// https://software.intel.com/en-us/node/503873
	float3 RGB_YCoCg(float3 c)
	{

		c = sqrt(c);
		
		// Y = R/4 + G/2 + B/4
		// Co = R/2 - B/2
		// Cg = -R/4 + G/2 - B/4
		return float3(
			 c.x/4.0 + c.y/2.0 + c.z/4.0,
			 c.x/2.0 - c.z/2.0,
			-c.x/4.0 + c.y/2.0 - c.z/4.0
		);
	}

	// https://software.intel.com/en-us/node/503873
	float3 YCoCg_RGB(float3 c)
	{
		// R = Y + Co - Cg
		// G = Y + Cg
		// B = Y - Co - Cg
		c = (float3(
			c.x + c.y - c.z,
			c.x + c.z,
			c.x - c.y - c.z
		));

		return c*c;
	}

	float4 clip_aabb(float3 aabb_min, float3 aabb_max, float4 p, float4 q)
	{
		// note: only clips towards aabb center (but fast!)
		float3 p_clip = 0.5 * (aabb_max + aabb_min);
		float3 e_clip = 0.5 * (aabb_max - aabb_min);

		float4 v_clip = q - float4(p_clip, p.w);
		float3 v_unit = v_clip.xyz / e_clip;
		float3 a_unit = abs(v_unit);
		float ma_unit = max(a_unit.x, max(a_unit.y, a_unit.z));

		if (ma_unit > 1.0)
			return float4(p_clip, p.w) + v_clip / ma_unit;
		else
			return q;// point inside aabb
	}

	float3 find_closest_fragment(float2 uv)
	{
		float2 closestFragment = uv;
		float closestDepth = 10000000.0;
		float2 sampleFragment = float2(0,0);
		float sampleDepth = 0;

		for( int i = 0; i < BlurKernelSamples; i++ ){
			sampleFragment = uv + BlurKernel[i].xy * _CameraDepthTexture_TexelSize.xy;
			sampleDepth = tex2Dlod(_CameraDepthTexture, float4( sampleFragment, 0, 0 ) ).x;
			if( sampleDepth < closestDepth ){
				closestDepth = sampleDepth;
				closestFragment = sampleFragment;
			}
		}

		return float3(closestFragment, closestDepth);
	}

	float4 sample_color( sampler2D tex, float2 uv ){
		float4 c = tex2Dlod( tex, float4(uv,0,0) );
		c = float4( RGB_YCoCg(c.xyz), c.w );
		return c;
	}

	float4 resolve_color(float4 c){
		c = float4( YCoCg_RGB( c.xyz ), c.w );
		return c;
	}
	
	half4 fragTAA (v2f IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float2 OneOverRTBSize = ( 1.0 / _RenderBufferSize.xy );
		
		half4 Scene = sample_color( _MainTex, ScreenUV );
		float3 closestUV = find_closest_fragment(ScreenUV);
		float2 motionVector = tex2Dlod( _MotionVector, float4(closestUV.xy,0,0) ).xy;

		float2 LastScreenUV = ScreenUV - ( motionVector * _MainTex_TexelSize.xy );
		half4 lastScene = sample_color( _AccumulatedFrames, LastScreenUV );

		float4 sceneMin = float4(10,10,10,10);
		float4 sceneMax = float4(-10,-10,-10,-10);
		float4 sceneAvg = float4(0,0,0,0);
		for( int i = 0; i < BlurKernelSamples; i++ ){
			float4 sampleScene = sample_color( _MainTex, ScreenUV + BlurKernel[i].xy * _MainTex_TexelSize.xy );
			sceneMin = min( sceneMin, sampleScene );
			sceneMax = max( sceneMax, sampleScene );
			sceneAvg += sampleScene;
		}
		sceneAvg *= 1.0 / BlurKernelSamples;

		float2 chroma_extent = 0.25 * 0.5 * (sceneMax.r - sceneMin.r);
		float2 chroma_center = Scene.gb;
		sceneMin.yz = chroma_center - chroma_extent;
		sceneMax.yz = chroma_center + chroma_extent;
		sceneAvg.yz = chroma_center;

		lastScene = clip_aabb(sceneMin.xyz, sceneMax.xyz, clamp(sceneAvg, sceneMin, sceneMax), lastScene);
		//lastScene = clamp( lastScene, sceneMin, sceneMax );

		float lumScene = Scene.r;
		float lumLastScene = lastScene.r;

		//float lumScene = Luminance(Scene.xyz);
		//float lumLastScene = Luminance(lastScene.xyz);

		float unbiased_diff = abs(lumScene - lumLastScene) / max(lumScene, max(lumLastScene, 0.2));
		float unbiased_weight = 1.0 - unbiased_diff;
		float unbiased_weight_sqr = unbiased_weight * unbiased_weight;
		float k_feedback = lerp(0.85, 0.95, unbiased_weight_sqr);

		//float k_feedback = lerp(_FeedbackMin, _FeedbackMax, unbiased_weight_sqr);
		//half blendAmount = tex2D( _BlendAmount, ScreenUV ).x;

		Scene = lerp( Scene, lastScene, k_feedback );
		Scene = resolve_color(Scene);
		return Scene;
	}


	half4 fragTAAOld (v2f IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float2 OneOverRTBSize = ( 1.0 / _RenderBufferSize.xy );
		
		half4 Scene = tex2D( _MainTex, ScreenUV );
		
		float2 motionVector = tex2Dlod( _MotionVector, float4(ScreenUV,0,0) ).xy;
		
		float2 LastScreenUV = ScreenUV - ( motionVector * OneOverRTBSize );
		
		half4 lastScene = tex2D( _AccumulatedFrames, LastScreenUV );
		
		half blendAmount = tex2D( _BlendAmount, ScreenUV ).x;
		
		//half blendAmount = 1;
		//half blendAmount2 = 0;
		//for( int i = 0; i < BlurKernelSamples; i++ ){	
		//	half blendSample = tex2D( _BlendAmount, ScreenUV + BlurKernel[i].xy * OneOverRTBSize );
		//	blendAmount = min( blendAmount, blendSample );
		//	blendAmount2 = max( blendAmount2, blendSample );
		//}
		
		return lerp( Scene, lastScene, blendAmount );
	}
	
	half4 fragTAADofPrepass (v2f IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float2 OneOverRTBSize = ( 1.0 / _RenderBufferSize.xy );
		
		half2 Depth = tex2D( _MainTex, ScreenUV ).xy;
		
		float2 motionVector = tex2Dlod( _MotionVector, float4(ScreenUV,0,0) ).xy;
		
		float2 LastScreenUV = ScreenUV - ( motionVector * OneOverRTBSize );
		
		half2 lastDepth = tex2D( _AccumulatedDofPrepass, LastScreenUV ).xy;
		
		half blendAmount = tex2D( _BlendAmount, ScreenUV ).x;
		
		return half4( lerp( Depth, lastDepth, blendAmount.xx ), 1, 1 );
	}	
	
	// Figure out how much to blur each pixel
	half4 PrepassDOF(v2f IN) : COLOR
	{		
		float2 ScreenUV = IN.uv;
		float2 Final;
		
		// sample the original scene and depth
		
		float depth = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ScreenUV );
		depth = min( LinearEyeDepth( depth ), 10000.0 );
		
		float depthDiff = ( depth - _FocalDepth );
		float closeBlur = smoothstep( -_DOFMaxDistance * 0.5, 0, depthDiff );
		float farBlur = smoothstep( _DOFMaxDistance, 0, depthDiff );
		
		float SourceBlurAmount = 1.0 - ( closeBlur * farBlur );
		
		//float SourceBlurAmount = saturate( abs( depth - _FocalDepth ) / _DOFMaxDistance );
		float localDepth = saturate( ( depth - _FocalDepth ) / ( _DOFMaxDistance * 2.0 ) + 0.5 );
		
		Final.x = pow( SourceBlurAmount, 0.5 );
		Final.y = localDepth;
		
		return half4( Final, 1, 1 );
	}
	
	
	ENDCG
		
	SubShader 
	{
		
		Pass
		{
			Name "Motion Vector"
			Tags { "RenderType"="Opaque" }
			ZTest off Cull Off ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment fragMotion
			#pragma target 3.0
			
			ENDCG
		}
		
		Pass
		{
			Name "Blend Pass"
			Tags { "RenderType"="Opaque" }
			ZTest off Cull Off ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment fragBlend
			#pragma target 3.0
			
			ENDCG
		}
		
		Pass
		{
			Name "TAA Pass"
			Tags { "RenderType"="Opaque" }
			ZTest off Cull Off ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment fragTAA
			#pragma target 3.0
			
			ENDCG
		}
		
		Pass
		{
			Name "TAA Prepass DOF Pass"
			Tags { "RenderType"="Opaque" }
			ZTest off Cull Off ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment fragTAADofPrepass
			#pragma target 3.0
			
			ENDCG
		}
		
		Pass
		{
			Name "Prepass DOF"
			Tags { "RenderType"="Opaque" }
			ZTest off Cull Off ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment PrepassDOF
			#pragma target 3.0
			
			ENDCG
		}
	} 
}
