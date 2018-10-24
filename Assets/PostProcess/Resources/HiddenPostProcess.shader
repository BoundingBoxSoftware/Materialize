// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/PostProcess" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	//Receive parameters
	uniform sampler2D	_MainTex;
	uniform float4		_MainTex_TexelSize;
	sampler2D_float 	_CameraDepthTexture;
	sampler2D_float		_AccumulatedDepth;
	sampler2D			_CameraNormalsTexture;
	sampler2D			_CameraDepthNormalsTexture;
	float4				_CameraDepthNormalsTexture_ST;
	
	sampler2D	_ScreenMip1;
	sampler2D	_ScreenMip2;
	sampler2D	_ScreenMip3;
	sampler2D	_ScreenMip4;
	sampler2D	_ScreenMip5;
	sampler2D	_ScreenMip6;
	sampler2D	_ScreenMip7;
	sampler2D	_ScreenMip8;
	sampler2D	_ScreenMip9;
	sampler2D	_ScreenMip10;
	sampler2D	_ScreenMip11;
	
	float		_ScreenX;
	float		_ScreenY;
	
	float2		_OneOverScreenSize;
	
	sampler2D	_VignetteTex;
	float 		_VignetteAmount;
	
	sampler2D	_FlareMaskTex;
	sampler2D	_FlareTex;
	float		_LensFlareAmount;
	
	sampler2D	_LensDirtTex;
	float		_LensDirtAmount;
	
	sampler2D	_NoiseTex;
	float		_NoiseAmount;
	
	sampler2D	_BloomTex;
	float		_AGBlendSpeed;
	float		_BloomThreshold;
	float		_BloomAmount;
	float		_BlurSpread;
	float4		_BlurDir;
	
	sampler2D	_GodRayTex;
	float 		_AspectRatio;
	
	sampler2D   _PrepassFocus;
	sampler2D   _ScreenDOFTexture;
	sampler2D   _ScreenDOFTexture2;
	float		_FocalDepth;
	float		_DOFBlur;
	float		_DOFMaxBlur;
	float		_DOFMaxDistance;
	float		_DOFMultiplier;
	float		_DOFMaxMultiplier;
	
	float		_MotionBlurAmount;
	sampler2D	_MotionTex;
	
	float4		_Offset;
	
	float4		_GodRaySourcePos;
	float4		_GodRayColor;
	float		_GodRaySize;
	float		_GodRayGlow;
	
	float3		_CamUp;
	float3		_CamRight;
	float3		_CamFwd;
	
	
	float		_SsaoAmount;
	sampler2D	_screenSSAOTexture;
	float4x4	_InvCameraMat;
	
	float4x4 	_ViewProjectInverse;
	float4x4	_CurrentCameraMatrix;
	float4x4	_LastCameraMatrix;
	float3		_PlayerCamPos;
	
	sampler2D _CameraGBufferTexture0;	// Diffuse color (RGB), MotionX (A)
	sampler2D _CameraGBufferTexture1;	// Matalic (R), Smoothness (G), Transmission (B), MotionY (A)
	sampler2D _CameraGBufferTexture2;	// World space normal (RGB), Coverage (A)
	sampler2D _CameraGBufferTexture3;	// ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer
	
	float4x4 _CURRENT_VP_MATRIX;
	float4x4 _INVERSE_VP_MATRIX;
	float4x4 _LAST_VP_MATRIX;
	float _FarClipPlane;


	float rand(float2 co){
	    return frac( sin( dot( co.xy ,float2(12.9898,78.233) ) ) * 43758.5453);
	}
	
	
	//======================================================//
	//					Composit Pass						//
	//======================================================//
	
	struct v2fCompose {
		float4 pos : SV_POSITION;
		float2 uv[3] : TEXCOORD0;
		
	};
	
	//Common Vertex Shader
	v2fCompose vertCompose( appdata_img v )
	{
		v2fCompose o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv[0] = v.texcoord.xy;
	 
		#if UNITY_UV_STARTS_AT_TOP
		if(_MainTex_TexelSize.y<0.0)
			o.uv[0].y = 1.0-o.uv[0].y;
		#endif
		
		o.uv[1] =  v.texcoord.xy;
		
		o.uv[2] =  floor( ( v.texcoord.xy * float2( _ScreenX, _ScreenY ) ) + ( sin( float2( _Time.y * 20.3, _Time.y * 12.7 ) ) * 128.0 ) ) * ( 1.0 / 256.0 );
	
		return o;
	
	} 

	half4 Compose(v2fCompose IN) : COLOR
	{		
		half2 VignetteUV = IN.uv[0];
		half2 ScreenUV = IN.uv[1];
		half2 NoiseUV = IN.uv[2];
		float4 Final;
		
		float4 averageColor = tex2D( _ScreenMip11, ScreenUV );
		float averageLuminance = averageColor.x * 0.2 + averageColor.y * 0.5 + averageColor.z * 0.3;
		
		float targetLuminance = lerp( averageLuminance, 0.3, 0.1 );
		
		half4 VignetteTex = tex2D( _VignetteTex, VignetteUV );
		
		half4 NoiseTex = tex2D( _NoiseTex, NoiseUV );
		
		float SceneR = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 0.994 + 0.5 ).x;
		float SceneG = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 0.997 + 0.5 ).y;
		float SceneB = tex2D( _MainTex, ScreenUV ).z;
		
		float4 Scene = float4( SceneR, SceneG, SceneB, 1.0 );
		Scene = tex2D( _MainTex, ScreenUV );
		Scene.xyz *= targetLuminance / averageLuminance;
		
		float4 BloomTex = tex2D( _BloomTex, ScreenUV );	
		float4 FlareTex = tex2D( _FlareTex, ScreenUV );
		float4 LensDirtTex = tex2D( _LensDirtTex, ScreenUV );
		
		Final = Scene;

		Final += BloomTex * _BloomAmount;
		Final += ( BloomTex * _BloomAmount ) * LensDirtTex * _LensDirtAmount;
		
		Final += FlareTex * _LensFlareAmount;
		Final += ( FlareTex * _LensFlareAmount ) * LensDirtTex * _LensDirtAmount;
		
		Final *= lerp( float4(1,1,1,1), VignetteTex, _VignetteAmount );
		Final *= lerp( float4(1,1,1,1), NoiseTex + 0.5, _NoiseAmount );
		Final.w = 1.0; 

		Final.xyz *= rand( ScreenUV ) * 0.04 + 0.98;
		
		//Final.xyz = sqrt( Final.xyz );

		//Final.xyz = lerp( averageColor.xyz, Final.xyz, 0.1 );
		
		return Final;
	}
	
	//======================================================//
	//					Mipping Pass						//
	//======================================================//
	
	struct v2fMipping {
		float4 pos : SV_POSITION;
		float2 uv[4] : TEXCOORD0;
		
	};
	
	v2fMipping vertMipping( appdata_img v )
	{
		v2fMipping o;
		o.pos = UnityObjectToClipPos (v.vertex);
		
		float2 uvCoords = v.texcoord.xy;
	 
		#if UNITY_UV_STARTS_AT_TOP
		if(_MainTex_TexelSize.y < 0.0)
			uvCoords.y = 1.0-uvCoords.y;
		#endif
		
		o.uv[0] =  uvCoords;
		o.uv[1] =  uvCoords + float2( 0.0, 1.0 / _ScreenY );
		o.uv[2] =  uvCoords + float2( 1.0 / _ScreenX, 0.0 );
		o.uv[3] =  uvCoords + float2( 1.0 / _ScreenX, 1.0 / _ScreenY );
	
		return o;
	
	} 
	
	half4 Mipping(v2fMipping IN) : COLOR
	{		
		half2 ScreenUV = IN.uv[1];
		float4 Final;
		
		float4 Scene = tex2D( _MainTex, IN.uv[0] );
		Scene += tex2D( _MainTex, IN.uv[1] );
		Scene += tex2D( _MainTex, IN.uv[2] );
		Scene += tex2D( _MainTex, IN.uv[3] );
		
		Scene *= 0.25;
		
		Final = clamp( Scene, 0.0001, 100.0 );
		Final.w = 0.0; // for depth of field
	  
		return Final;
	}
	
	//======================================================//
	//						Threshold						//
	//				uses mipping vertex data				//
	//======================================================//
	
	
	float4 AdaptiveGlare(v2fMipping IN) : COLOR
	{		
		half2 ScreenUV = IN.uv[0];
	
		float4 Final = tex2D( _MainTex, ScreenUV );

		Final.w = _AGBlendSpeed;
	  
		return Final;
	}
	
	half4 Threshold(v2fMipping IN) : COLOR
	{		
		half2 ScreenUV = IN.uv[0];
		float4 Final;
		
		float4 averageColor = tex2D( _ScreenMip11, ScreenUV );
		
		float4 threshold = ( averageColor.x * 0.3 + averageColor.y * 0.5 + averageColor.z * 0.2 ) + _BloomThreshold;
		
		float4 Scene = max( tex2D( _MainTex, IN.uv[0] ) - threshold, 0.0 );
		Scene += max( tex2D( _MainTex, IN.uv[1] ) - threshold, 0.0 );
		Scene += max( tex2D( _MainTex, IN.uv[2] ) - threshold, 0.0 );
		Scene += max( tex2D( _MainTex, IN.uv[3] ) - threshold, 0.0 );
		
		Scene *= 0.25;
		Scene.w = 1.0;
	  
		return Scene;
	}
	
	//======================================================//
	//						Blur Pass						//
	//														//
	//======================================================//
	
	
	struct v2fBlur {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		
	};
	
	//Common Vertex Shader
	v2fBlur vertBlur( appdata_img v )
	{
		v2fBlur o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv = v.texcoord.xy;
	
		return o;
	
	} 
	
	half4 Blur(v2fBlur IN) : COLOR
	{		
		half2 ScreenUV = IN.uv;
		float4 Final;
		
		float2 blurDir = _BlurDir.xy;
		float2 pixelSize = float2( 1.0 / _ScreenX, 1.0 / _ScreenY );
		
		float4 Scene = tex2D( _MainTex, ScreenUV ) * 0.1438749;
		
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * _BlurSpread ) ) * 0.1367508;
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * 2.0 * _BlurSpread ) ) * 0.1167897;
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * 3.0 * _BlurSpread ) ) * 0.08794503;
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * 4.0 * _BlurSpread ) ) * 0.05592986;
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * 5.0 * _BlurSpread ) ) * 0.02708518;
		Scene += tex2D( _MainTex, ScreenUV + ( blurDir * pixelSize * 6.0 * _BlurSpread ) ) * 0.007124048;
		
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * _BlurSpread ) ) * 0.1367508;
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * 2.0 * _BlurSpread ) ) * 0.1167897;
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * 3.0 * _BlurSpread ) ) * 0.08794503;
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * 4.0 * _BlurSpread ) ) * 0.05592986;
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * 5.0 * _BlurSpread ) ) * 0.02708518;
		Scene += tex2D( _MainTex, ScreenUV - ( blurDir * pixelSize * 6.0 * _BlurSpread ) ) * 0.007124048;
		
		Final = Scene;
		Final.w = 1.0;
	  
		return Final;
	}
	
	//======================================================//
	//						Flare Pass						//
	//														//
	//======================================================//
	
	
	struct v2fFlare {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		
	};
	
	//Common Vertex Shader
	v2fFlare vertFlare( appdata_img v )
	{
		v2fFlare o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv = v.texcoord.xy;
	
		return o;
	
	} 
	
	half4 Flare(v2fFlare IN) : COLOR
	{		
		half2 ScreenUV = IN.uv;
		float3 Final = 0.0;
		
		// Blue Outer
		float3 BlueOuter = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 0.6 + 0.5  ).xyz;
		
		// Red Outer
		float3 RedOuter = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 0.8 + 0.5  ).xyz;
		
		// Purple Inner
		float3 PurpleInner = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 2.0 + 0.5  ).xyz * tex2Dlod( _FlareMaskTex, float4( ( ScreenUV - 0.5 ) * 2.0 + 0.5, 0, 0 )  ).xyz;
		
		// Green Inner
		float3 GreenInner = tex2D( _MainTex, ( ScreenUV - 0.5 ) * 6.0 + 0.5  ).xyz * tex2Dlod( _FlareMaskTex, float4( ( ScreenUV - 0.5 ) * 6.0 + 0.5, 0, 0 )  ).xyz;
		
		// Yellow Inv Inner
		float3 InvYellowInner = tex2D( _MainTex, ( ( 1.0 - ScreenUV ) - 0.5 ) * 3.0 + 0.5 ).xyz * tex2Dlod( _FlareMaskTex, float4( ( ( 1.0 - ScreenUV ) - 0.5 ) * 3.0 + 0.5, 0, 0 ) ).xyz;
		
		
		// Inv Red Outer
		float3 InvRedOuter = tex2D( _MainTex, ( ( 1.0 - ScreenUV ) - 0.5 ) * 1.0 + 0.5  ).xyz;
		
		// Inv Blue Outer
		float3 InvBlueOuter = tex2D( _MainTex, ( ( 1.0 - ScreenUV ) - 0.5 ) * 0.5 + 0.5  ).xyz;

		
		Final += BlueOuter * float3( 0.1, 0.3, 1.0 ) * 1.0;
		Final += RedOuter * float3( 1.0, 0.3, 0.1 ) * 0.7;
		Final += PurpleInner * float3( 0.7, 0.1, 1.0 ) * 0.7;
		Final += GreenInner * float3( 0.1, 0.9, 1.0 ) * 1.0;
		Final += InvYellowInner * float3( 0.1, 0.9, 0.2 ) * 1.0;
		Final += InvRedOuter * float3( 1.0, 0.5, 0.3 ) * 0.7;
		Final += InvBlueOuter * float3( 0.3, 0.5, 1.0 ) * 1.0;
	  
		return float4( Final, 1.0 );
	}
	
	
	//======================================================//
	//					Prepass DOF Pass					//
	//														//
	//						DOF Pass						//
	//														//
	//======================================================//
	
	
	struct v2fDOF {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uvNoise : TEXCOORD1;
		
	};
	
	//Common Vertex Shader
	v2fDOF vertDOF( appdata_img v )
	{
		v2fDOF o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv = v.texcoord.xy;
		
		float2 screenSize = float2( _ScreenX, _ScreenY );
		o.uvNoise = v.texcoord.xy *  ( screenSize / 256.0 ) + floor( _Time.y * float2( 9.67, 12.38 ) * 60.0 ) * 0.0125;
	
		return o;
	
	}
	
	// Figure out how much to blur each pixel
	half4 PrepassDOF(v2fDOF IN) : COLOR
	{		
		float2 ScreenUV = IN.uv;
		float2 Final;
		
		// sample the original scene and depth
		
		//float depth = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ScreenUV );
		float depth = SAMPLE_DEPTH_TEXTURE( _AccumulatedDepth, ScreenUV );
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
	
	static const int DOFKernelSamples = 6;		
	static const float2 DOFKernel[DOFKernelSamples] =
	{
		float2(-0.5,1.0),
		float2(0.5,1.0),
		float2(-1.0,0.0),
		float2(1.0,0.0),
		float2(-0.5,-1.0),
		float2(0.5,-1.0)
	};
	
	static const int DOFKernelSamplesHigh= 10;		
	static const float2 DOFKernelHigh[DOFKernelSamplesHigh] =
	{
		float2(-0.75,1.5),
		float2(0.75,1.5),
		float2(-1.5,0.0),
		float2(1.5,0.0),
		float2(-0.75,-1.5),
		float2(0.75,-1.5),
		float2(-0.75,0.75),
		float2(0.75,0.75),
		float2(-0.75,-0.75),
		float2(0.75,-0.75)
	};
	
	static const int DOFKernelSamplesLow = 4;		
	static const float2 DOFKernelLow[DOFKernelSamplesLow] =
	{
		float2(-1.0,-1.0),
		float2(-1.0,1.0),
		float2(1.0,1.0),
		float2(1.0,-1.0)
	};
	
	void sampleIntegrate( inout half4 Scene, inout half MaxContribution, half2 SourceBlurAmount, float2 ScreenUV, float4 pixelInfo, float2 kernelOffset, half sourceBlurContribute ){
	//half4 sampleIntegrate( half2 SourceBlurAmount, float2 ScreenUV, float2 pixelSize, float2 kernelOffset, half sourceBlurContribute ){
		
		ScreenUV = ScreenUV + kernelOffset * pixelInfo.xy * _DOFMultiplier;
		ScreenUV = round( ScreenUV * pixelInfo.zw ) * pixelInfo.xy; // select the center of a pixel

		half4 newScene = tex2Dlod( _MainTex, float4( ScreenUV, 0, 0 ) );
		half2 newBlurAmount = tex2Dlod( _PrepassFocus, float4( ScreenUV, 0, 0 ) ).xy;
		
		newBlurAmount.x *= min( _DOFMaxBlur, _DOFMaxMultiplier );
		
		half isInFront = clamp( ( newBlurAmount.y - SourceBlurAmount.y ) * 100000.0, -1.0, 1.0 );

		half newBlurContribute = smoothstep( _DOFMultiplier / 2.0, _DOFMultiplier, newBlurAmount.x );
		
		half blurDiff = ( 1.0 - abs( sourceBlurContribute - newBlurContribute ) ) * sourceBlurContribute;
		half contributionA = lerp( newBlurContribute, blurDiff, saturate( isInFront ) );
		half contributionB = lerp( sourceBlurContribute, blurDiff, saturate( -isInFront ) );
		half contribution = lerp( contributionA, contributionB, pow( _DOFMultiplier / 16, 0.5 ) );
		
		contribution = clamp( contribution, 0.0001, 1.0 );
		
		newScene.xyz *= contribution;
		newScene.w = contribution;

		//return newScene;
		
		Scene += newScene;
		MaxContribution = max( MaxContribution, contribution );
	}
	
	half4 DOF(v2fDOF IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float4 Final;
		float2 screenSize = float2( _ScreenX, _ScreenY );
		float2 pixelSize = float2( 1.0 / _ScreenX, 1.0 / _ScreenY );
		
		half2 SourceBlurAmount = tex2Dlod( _PrepassFocus, float4( ScreenUV, 0, 0 ) ).xy;	
		
		half noise = rand( ScreenUV );
		
		SourceBlurAmount.x *= min( _DOFMaxBlur, _DOFMaxMultiplier );
		half sourceBlurContribute = smoothstep( _DOFMultiplier / 2.0, _DOFMultiplier, SourceBlurAmount.x );
		
		//pixelSize *= _DOFMultiplier;// * ( ( noise - 0.5 ) * 0.5 + 1.0 );
		
		half4 Scene = tex2Dlod( _MainTex, float4( ScreenUV, 0, 0 ) );
		half maxContribution = Scene.w;
		Scene.w = 1.0;
		
		
		//Scene += sampleIntegrate( SourceBlurAmount, ScreenUV, pixelSize, lerp( DOFKernelLow[0], DOFKernelLow[1], noise ), sourceBlurContribute );
		//Scene += sampleIntegrate( SourceBlurAmount, ScreenUV, pixelSize, lerp( DOFKernelLow[1], DOFKernelLow[2], noise ), sourceBlurContribute );
		//Scene += sampleIntegrate( SourceBlurAmount, ScreenUV, pixelSize, lerp( DOFKernelLow[2], DOFKernelLow[3], noise ), sourceBlurContribute );
		//Scene += sampleIntegrate( SourceBlurAmount, ScreenUV, pixelSize, lerp( DOFKernelLow[3], DOFKernelLow[0], noise ), sourceBlurContribute );	
		
		sampleIntegrate( Scene, maxContribution, SourceBlurAmount, ScreenUV, float4( pixelSize, screenSize ), lerp( DOFKernelLow[0], DOFKernelLow[1], noise ), sourceBlurContribute );
		sampleIntegrate( Scene, maxContribution, SourceBlurAmount, ScreenUV, float4( pixelSize, screenSize ), lerp( DOFKernelLow[1], DOFKernelLow[2], noise ), sourceBlurContribute );
		sampleIntegrate( Scene, maxContribution, SourceBlurAmount, ScreenUV, float4( pixelSize, screenSize ), lerp( DOFKernelLow[2], DOFKernelLow[3], noise ), sourceBlurContribute );
		sampleIntegrate( Scene, maxContribution, SourceBlurAmount, ScreenUV, float4( pixelSize, screenSize ), lerp( DOFKernelLow[3], DOFKernelLow[0], noise ), sourceBlurContribute );
		
		Scene.xyz *= 1.0 / Scene.w;
		Scene.w = maxContribution;
		
		return Scene;
	}
	
	half4 DOFComp(v2fDOF IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		
		half2 SourceBlurAmount = tex2Dlod( _PrepassFocus, float4( ScreenUV, 0, 0 ) ).xy;
		
		SourceBlurAmount.x *= min( _DOFMaxBlur, _DOFMaxMultiplier );
		half sourceBlurContribute = smoothstep( _DOFMultiplier / 2.0, _DOFMultiplier, SourceBlurAmount.x );
		
		half4 SceneLow = tex2Dlod( _ScreenMip1, float4( ScreenUV, 0, 0 ) );
		half4 Scene = tex2Dlod( _MainTex, float4( ScreenUV, 0, 0 ) );
		
		Scene = lerp ( Scene, SceneLow, max( sourceBlurContribute, SceneLow.w ) );
		Scene.w = 1.0;
		//return float4( SceneLow.www, 1.0 );
		return Scene;
	}
	
	//======================================================//
	//					God Ray Pass						//
	//														//
	//======================================================//
	
	
	struct v2fGodRay {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		
	};
	
	//Common Vertex Shader
	v2fGodRay vertGodRay( appdata_img v )
	{
		v2fGodRay o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv = v.texcoord.xy;
	
		return o;
	
	} 
	
	half4 GodRay(v2fGodRay IN) : COLOR
	{		
		half2 ScreenUV = IN.uv;
		
		float2 GodRayOffset = ScreenUV - _GodRaySourcePos.xy;
		
		float4 GodRayTex = tex2D( _MainTex, ScreenUV );
		int i = 0;
		int Passes = 30;
		float onOverPasses = 1.0 / Passes;
		for( i = 0; i < Passes; i++ ){
			GodRayTex += ( tex2D( _MainTex, ScreenUV - GodRayOffset * 0.03 * i ) * pow( ( 1.0 - ( i * onOverPasses ) ), 0.3 ) );
			
		}
		GodRayTex *= 0.05;
	  
		return GodRayTex; 
	}
	
	half4 Clear(v2fGodRay IN) : COLOR
	{			  
		return float4( 0,0,0,0 );
	}
	
	//======================================================//
	//					God Ray Clip Pass					//
	//														//
	//======================================================//
	
	
	half4 GodRayClip(v2fGodRay IN) : COLOR
	{		
		if( _GodRaySourcePos.z < 0 ){ return float4(0,0,0,0); }
		
		half2 ScreenUV = IN.uv;
		
		//float depth = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ScreenUV );
		float depth = SAMPLE_DEPTH_TEXTURE( _AccumulatedDepth, ScreenUV );
		depth = LinearEyeDepth( depth );

		float GodRayOffset = length( ( ScreenUV - _GodRaySourcePos.xy ) * float2( _AspectRatio, 1.0 ) );
		
		float scaler = ( 1.0 / _GodRaySize );
		
		float GodRayOuter = smoothstep( 0.5, 0.3, saturate( GodRayOffset * scaler ) );
		
		float GodRayInner = smoothstep( 0.3, 0.0, saturate( GodRayOffset * scaler ) );
		
		float GodRayDepthOffset = ( depth - _GodRaySourcePos.z );
		
		// if the source is really far away than consider it the sun
		float sunMask = saturate( _GodRaySourcePos.z - 1000.0 );
		
		float GodRayCloseMask = 1.0 - saturate( -GodRayDepthOffset * lerp( 0.2, 100.0, sunMask ) ) ;
		float GodRayFarMask = lerp( 1.0 - saturate( GodRayDepthOffset * 0.2 ), 1.0, sunMask ) ;
		
		float4 GodRayTex = tex2D( _MainTex, ScreenUV );
		float4 GodRayTexExtra = tex2D( _ScreenMip3, ScreenUV );
		GodRayTex += GodRayTexExtra * 0.5;
		GodRayTex += _GodRayGlow;
		GodRayTex *= _GodRayColor;
		
		float4 EdgeMask = tex2D( _FlareMaskTex, ScreenUV  );
		
		float4 Final = GodRayTex * max( GodRayOuter * GodRayCloseMask * GodRayFarMask, GodRayInner * GodRayCloseMask ) * pow( EdgeMask.x, 0.2 );
		//Final = max( GodRayOuter * GodRayDepthOffset, GodRayInner );
	  
		return Final;
	}
	
	//======================================================//
	//				Ambient Occlusion Pass					//
	//														//
	//======================================================//


	struct v2fAO {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		
	};
	
	//Common Vertex Shader
	v2fAO vertAO( appdata_img v )
	{
		v2fAO o;
		o.pos = UnityObjectToClipPos (v.vertex);
		o.uv = v.texcoord.xy;
		o.uv2 = v.texcoord.xy * float2( _ScreenX, _ScreenY ) / 256.0;
	
		return o;
	
	}
	
	half4 sceneAOFetch( float4 normalDepth, float2 ScreenUV, sampler2D normalDepthSample ){

		
		float4 sampleNormalDepth = tex2D( normalDepthSample, ScreenUV );
		
		sampleNormalDepth.w = DecodeFloatRG( sampleNormalDepth.zw ) * 1000;
		sampleNormalDepth.xy = sampleNormalDepth.xy * 2.0 - 1.0;
		sampleNormalDepth.z = sqrt( 1.0 - saturate( dot( sampleNormalDepth.xy, sampleNormalDepth.xy ) ) );
		
		half normalDot = dot( normalDepth.xyz, sampleNormalDepth.xyz );
		half depthDiff = clamp( -1.0, 1.0, ( sampleNormalDepth.w - normalDepth.w ) * 5.0 ) * 0.5 + 0.5;
		half depthContribution = 1.0 - saturate( abs( sampleNormalDepth.w - normalDepth.w ) / normalDepth.w * 5.0 );
		
		half AO = normalDot * saturate( depthDiff + 0.5 );
		
		AO = lerp( 1.0, AO, depthContribution );
		
		return half4( AO, AO, AO, 1.0 );
	}
	
	half4 AO(v2fAO IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float4 Final;
		float pixelSpread = 20.0;
		
		float4 noiseTex = tex2D( _NoiseTex, IN.uv2 );
		
		float2 pixelSize = float2( 1.0 / _ScreenX, 1.0 / _ScreenY );
		float2 distMultiply = pixelSize * pixelSpread;
			
		float4 normalDepth = tex2D( _CameraDepthNormalsTexture, ScreenUV );
		
		normalDepth.w = DecodeFloatRG( normalDepth.zw ) * 1000 + ( noiseTex.z - 0.5 ) * 0.1;
		normalDepth.xy = normalDepth.xy * 2.0 - 1.0;
		normalDepth.z = sqrt( 1.0 - saturate( dot( normalDepth.xy, normalDepth.xy ) ) );
		
		float2 normalOffset = ( normalDepth.xy * pixelSpread * pixelSize * 5.0 ) + ( noiseTex.xy - 0.5 ) * 0.1;

		
		//     01,02,03
		//  04,05,06,07,08
		//  09,10,11,12,13
		//  14,15,16,17,18
		//     19,20,21
		
		half4 Scene = float4( 0,0,0,0 );
		Scene.xyz = normalDepth.xyz;
		
		half4 AO = 0.0;
		
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-1,2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(0,2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(1,2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-2,1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-1,1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(0,1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(1,1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(2,1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
			
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-2,0) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-1,0) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(0,0) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(1,0) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(2,0) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
	
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-2,-1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-1,-1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(0,-1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(1,-1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(2,-1) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(-1,-2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(0,-2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		AO += sceneAOFetch( normalDepth, ScreenUV + ( float2(1,-2) * distMultiply + normalOffset ), _CameraDepthNormalsTexture );
		
		AO.xyz *= ( 1.0 / AO.w );
		AO.w = 1.0;
		
		return AO;
	}
	
	half4 AOComp(v2fAO IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float4 Final;
		
		float2 screenSize = 1.0 / float2( _ScreenX, _ScreenY );
		
		float ssao = 0.0;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 1.0, -1.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 0.0, -1.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( -1.0, -1.0 ) ).r;
		
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 1.0, 0.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 0.0, 0.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( -1.0, 0.0 ) ).r;
		
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 1.0, 1.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( 0.0, 1.0 ) ).r;
		ssao += tex2D( _screenSSAOTexture, ScreenUV + screenSize * float2( -1.0, 1.0 ) ).r;
		
		ssao *= 0.11111;
		float4 Scene = tex2D( _MainTex, ScreenUV );
		Scene.xyz *= lerp( 1.0, ssao * ssao, _SsaoAmount );
		
		return Scene;
	}
	
	
	//======================================================//
	//					Average Color Pass					//
	//														//
	//======================================================//


	struct v2fAvgColor {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;		
	};
	
	//Common Vertex Shader
	v2fAvgColor vertAvgColor( appdata_img v )
	{
		v2fAvgColor o;
		o.pos = UnityObjectToClipPos (v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}
	
	half4 AvgColor(v2fAvgColor IN) : COLOR
	{

		float4 avgColor = 0.0;
		float2 UV = IN.uv;
		
		avgColor += tex2D( _MainTex, UV );
		avgColor += tex2D( _MainTex, UV + float2( _OneOverScreenSize.x, 0 ) );
		avgColor += tex2D( _MainTex, UV + float2( 0, _OneOverScreenSize.y ) );
		avgColor += tex2D( _MainTex, UV + float2( _OneOverScreenSize.xy ) );

		
		return avgColor;
	}
	
	//======================================================//
	//						Motion Blur						//
	//														//
	//======================================================//


	struct v2fMBlur {
		float4 pos : SV_POSITION;	
		float2 uv : TEXCOORD0;
		float4 screenPos : TEXCOORD1;
		float4 lastScreenPos : TEXCOORD2;
	};
	
	//Common Vertex Shader
	v2fMBlur vertMBlur( appdata_img v )
	{
		v2fMBlur o;
		o.pos = UnityObjectToClipPos (v.vertex);
	
		o.uv = v.texcoord.xy;
		
		float2 screenSize = float2( _ScreenX, _ScreenY );

		float4 screenPos = float4( v.texcoord.xy * 2 - 1, 0, 1 );
		float4 worldPos = mul( _INVERSE_VP_MATRIX, screenPos );
		float4 lastScreenPos = mul( _LAST_VP_MATRIX, worldPos );
		
		o.screenPos = ComputeScreenPos( screenPos );
		o.lastScreenPos = ComputeScreenPos( lastScreenPos );
		
		return o;
	}
	
	
	half4 MotionBlur(v2fMBlur IN) : COLOR
	{
		float2 ScreenUV = IN.uv;
		float4 Final;
		float2 screenSize = float2( _ScreenX, _ScreenY );
		float2 pixelSize = float2( 1.0 / _ScreenX, 1.0 / _ScreenY );
		
		half2 motionVector = tex2Dlod( _MotionTex, float4(ScreenUV,0,0) ).xy;
		
		/*
		half2 motionVector = half2(0,0);
		
		half coverage = tex2Dlod( _CameraGBufferTexture2, float4(ScreenUV,0,0) ).w;	
		if( coverage == 0 ){
			float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
			screenPos.y = 1.0 - screenPos.y;
			float2 lastScreenPos = IN.lastScreenPos.xy / IN.lastScreenPos.w;
			lastScreenPos.y = 1.0 - lastScreenPos.y;
			motionVector = ( screenPos - lastScreenPos );
			motionVector = motionVector * screenSize * ( 1.0/64.0 );
		}else{
			motionVector = half2( tex2Dlod( _CameraGBufferTexture0, float4(ScreenUV,0,0) ).w, tex2Dlod( _CameraGBufferTexture1, float4(ScreenUV,0,0) ).w ) * 2.0 - 1.0;
			float2 mvStep = step( float2(0,0), motionVector );
			motionVector = ( motionVector * motionVector );
			motionVector = lerp( -motionVector, motionVector, mvStep );
		}
		*/
		
		half2 finalMBVector = motionVector * _MotionBlurAmount * pixelSize * 0.1;
		
		half4 Scene = 0.0;
		
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * -4.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * -3.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * -2.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * -1.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * 0.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * 1.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * 2.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * 3.0, 0, 0 ) );
		Scene += tex2Dlod( _MainTex, float4( ScreenUV + finalMBVector * 4.0, 0, 0 ) );
		
		return Scene / Scene.w;

	}



	ENDCG 
	
	Subshader {

		ZTest Off
		Cull Off
		ZWrite Off
		Fog { Mode off }
		
		//Pass 0 Composit of all passes
		Pass 
		{
			Name "Compose"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertCompose
			#pragma fragment Compose
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 1 Mipping pass
		Pass 
		{
			Name "Mipping"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertMipping
			#pragma fragment Mipping
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 2 Threshold pass
		Pass 
		{
			Name "Threshold"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertMipping
			#pragma fragment Threshold
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 3 Blur pass
		Pass 
		{
			Name "Blur"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertBlur
			#pragma fragment Blur
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 4 Flare pass
		Pass 
		{
			Name "Flare"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertFlare
			#pragma fragment Flare
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 5 PrepassDOF pass
		
		Pass 
		{
			Name "PrepassDOF"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertDOF
			#pragma fragment PrepassDOF
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 6 DOF pass
		Pass 
		{
			Name "DOF"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertDOF
			#pragma fragment DOF
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 7 DOFComp pass
		Pass 
		{
			Name "DOFComp"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertDOF
			#pragma fragment DOFComp
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 8 God Ray pass
		Pass 
		{
			Name "GodRay"
			
			Blend One One // Additive blending
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertGodRay
			#pragma fragment GodRay
			#pragma target 3.0		
			ENDCG
		}
		
		//Pass 9 God Ray Clip pass
		Pass 
		{
			Name "GodRayClip"
		
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertGodRay
			#pragma fragment GodRayClip
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 10 Clear pass
		Pass 
		{
			Name "Clear"
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertGodRay
			#pragma fragment Clear
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 11 Adaptive Glare Threshold pass
		Pass 
		{
			Name "AdaptiveGlare"
			
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertMipping
			#pragma fragment AdaptiveGlare
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 12 Ambient Occlusion pass
		Pass 
		{
			Name "AO"
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertAO
			#pragma fragment AO
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 13 Ambient Occlusion Comp pass
		Pass 
		{
			Name "AOComp"
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertAO
			#pragma fragment AOComp
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 14 Average Color pass
		Pass 
		{
			Name "AvgColor"
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertAvgColor
			#pragma fragment AvgColor
			#pragma target 3.0
			ENDCG
		}
		
		//Pass 15 Motion Blur
		Pass
		{
			Name "MotionBlur"
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vertMBlur
			#pragma fragment MotionBlur
			#pragma target 3.0
			ENDCG
		}
	}
}
