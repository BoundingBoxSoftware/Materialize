// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Shader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "Photoshop.cginc"
	
	sampler2D _MainTex;
	sampler2D _HeightTex;
	sampler2D _LightTex;
	sampler2D _LightBlurTex;
	sampler2D _DiffuseTex;
	
	float _BlurScale;
	float4 _ImageSize;
	float _GamaCorrection;
	
	float4 _BlurDirection;
	int _BlurSamples;
	float _BlurSpread;
	
	sampler2D _BlurTex0;
	sampler2D _BlurTex1;
	sampler2D _BlurTex2;
	sampler2D _BlurTex3;
	sampler2D _BlurTex4;
	sampler2D _BlurTex5;
	sampler2D _BlurTex6;
	
	float _Contrast;
	float _BlurContrast;
	
	float _Blur0Weight;
	float _Blur0Contrast;

	float _Blur1Weight;
	float _Blur1Contrast;

	float _Blur2Weight;
	float _Blur2Contrast;

	float _Blur3Weight;
	float _Blur3Contrast;

	float _Blur4Weight;
	float _Blur4Contrast;

	float _Blur5Weight;
	float _Blur5Contrast;

	float _Blur6Weight;
	float _Blur6Contrast;

	
	
	float _Pinch;
	float _Pillow;
	float _EdgeAmount;
	float _CreviceAmount;
	
	float _IsColor;
	
	float _Spread;
	float _Depth;
	float _AOBlend;
	
	sampler2D _BlendTex;
	float _BlendAmount;
	
	float _DiffuseContrast;
	float _DiffuseBias;
		
	sampler2D _BlurTex;
	float _BlurWeight;

	sampler2D _AvgTex;
		
	float _LightMaskPow;
	float _LightPow;
		
	float _DarkMaskPow;
	float _DarkPow;

	float _HotSpot;
	float _DarkSpot;

	float _FinalContrast;
	float _FinalBias;
	float _FinalGain;
	
	float _AngularIntensity;
	float _Angularity;
		
	float _ColorLerp;
		
	float _Saturation;

	float _ShapeRecognition;
	float _LightRotation;
	float _ShapeBias;
	
	int _FlipNormalY;
	float _HeightFromNormal;
	int _Desaturate;


	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	v2f vert(appdata_img v) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}
	
	float4 fragDesaturate (v2f IN) : SV_Target
	{
		
		float2 UV = IN.uv;
		half4 mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) );
		mainTex.x = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
		
		return float4( mainTex.xxx, 1.0 );
	}
	
	
	float4 fragBlur (v2f IN) : SV_Target
	{
	
		float2 pixelSize = ( 1.0 / _ImageSize.xy );
		float2 UV = IN.uv;

		int totalSamples = _BlurSamples * 2;
		int i;
		half4 mainTex = float4(0,0,0,0);
		for( i = -_BlurSamples; i <= _BlurSamples; i++ ){

			float weight = cos( ( float( i ) / float( totalSamples ) ) * 6.28318530718 ) * 0.5 + 0.5;
			half4 sampleTex = tex2Dlod(_MainTex, float4( UV.xy + pixelSize.xy * _BlurDirection.xy * i * _BlurSpread, 0, 0 ) );
			if( _Desaturate > 0 ){
				sampleTex.xyz = sampleTex.x * 0.3 + sampleTex.y * 0.5 + sampleTex.z * 0.2;
			}
			sampleTex = half4( sampleTex.xyz * weight, weight );
			mainTex += sampleTex;
		
		}
		
		mainTex.xyz *= 1.0 / mainTex.w;
		
		mainTex.xyz = saturate( ( ( mainTex.xyz - 0.5 ) * _BlurContrast ) + 0.5 );
		
		return float4( mainTex.xyz, 1.0 );
	}
	

	
	float4 fragCombineHeight (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		half4 heightTex = half4(0, 0, 0, 0);
		
		if (_HeightFromNormal > 0.5) {
			heightTex = tex2Dlod(_MainTex, float4(UV, 0, 0)).xxxx;
		} else {
			float avgColor = pow( tex2Dlod(_AvgTex, float4(UV, 0, 0)).x, 0.45 );

			heightTex = half4((pow(tex2Dlod(_BlurTex0, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur0Contrast + 0.5, 1.0) * _Blur0Weight;
			half4 blurTex1 = half4((pow(tex2Dlod(_BlurTex1, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur1Contrast + 0.5, 1.0) * _Blur1Weight;
			half4 blurTex2 = half4((pow(tex2Dlod(_BlurTex2, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur2Contrast + 0.5, 1.0) * _Blur2Weight;
			half4 blurTex3 = half4((pow(tex2Dlod(_BlurTex3, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur3Contrast + 0.5, 1.0) * _Blur3Weight;
			half4 blurTex4 = half4((pow(tex2Dlod(_BlurTex4, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur4Contrast + 0.5, 1.0) * _Blur4Weight;
			half4 blurTex5 = half4((pow(tex2Dlod(_BlurTex5, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur5Contrast + 0.5, 1.0) * _Blur5Weight;
			half4 blurTex6 = half4((pow(tex2Dlod(_BlurTex6, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur6Contrast + 0.5, 1.0) * _Blur6Weight;

			heightTex = heightTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;

			heightTex *= 1.0 / heightTex.w;
		}
		
		heightTex.x = saturate(((heightTex.x - 0.5) * _FinalContrast) + 0.5 + _FinalBias);
					
		heightTex.x = saturate( heightTex.x );
		
		if( heightTex.x > 0.5 ){
			heightTex.x = pow( saturate( heightTex.x * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5;
		}else{
			heightTex.x = 1.0 - ( pow( saturate( ( 1.0 - heightTex.x ) * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5 );
		}
		
		
		return float4( heightTex.xxx, 1.0 );
	}

	
	float4 fragNormal (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;
		float2 pixelSize = 1.0 / _ImageSize.xy;
		float4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
		float4 mainTexDDX = tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y, 0, 0 ) );
		float4 mainTexDDY = tex2Dlod(_MainTex, float4( UV.x, UV.y + pixelSize.y, 0, 0 ) );
		
		mainTexDDX = mainTexDDX - mainTex;
		mainTexDDY = mainTexDDY - mainTex;
		
		float3 normalTex = normalize( cross( normalize( float3( 1.0, 0.0, mainTexDDX.x * _BlurContrast ) ), normalize( float3( 0.0, 1.0, mainTexDDY.x * _BlurContrast ) ) ) );
		
		half3 heightTex = tex2Dlod(_LightTex, float4( UV, 0, 0 ) ).xyz;
		half3 heightBlurTex = tex2Dlod(_LightBlurTex, float4( UV, 0, 0 ) ).xyz;
		if( _Desaturate > 0 ){
			heightTex.x = heightTex.x * 0.3 + heightTex.y * 0.5 + heightTex.z * 0.2;
		}
		half HPHeight = ( heightTex.x - heightBlurTex.x ) + _ShapeBias;
		HPHeight = HPHeight * 2.0 - 1.0;
		
		float3 lightDirection = float3( sin( _LightRotation ), cos( _LightRotation ), 0 );
		float3 lightCrossDirection = cross( lightDirection, float3(0,0,1) );
		float3 shape = ( HPHeight * lightDirection ) + ( dot( normalTex, lightCrossDirection ) * lightCrossDirection );
		shape.z = sqrt( 1.0 - saturate( dot(shape.xy, shape.xy ) ) );
		shape = normalize( shape );

		normalTex = normalize( lerp( normalTex, shape, _ShapeRecognition ) );

		normalTex = normalTex * 0.5 + 0.5;

		return float4( normalTex, 1.0 );
	}
	
	
	float4 fragCombineNormal (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		half4 mainTex = half4( tex2Dlod(_BlurTex0, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur0Weight;
		half4 blurTex1 = half4( tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur1Weight;
		half4 blurTex2 = half4( tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur2Weight;
		half4 blurTex3 = half4( tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur3Weight;
		half4 blurTex4 = half4( tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur4Weight;
		half4 blurTex5 = half4( tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur5Weight;
		half4 blurTex6 = half4( tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur6Weight;
		
		mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
		
		mainTex *= 1.0 / mainTex.w;
		
		mainTex.xyz = normalize( mainTex.xyz * 2.0 - 1.0 );

		float3 angularDir = normalize( float3( normalize( float3( mainTex.xy, 0.001 ) ).xy * _AngularIntensity, max( 1.0 - _AngularIntensity, 0.001 ) ) );
		mainTex.xyz = lerp( mainTex.xyz, angularDir, _Angularity );
		
		mainTex.xy = mainTex.xy * _FinalContrast;
		mainTex.z = pow( saturate( mainTex.z ), _FinalContrast );
		
		mainTex.xyz = normalize( mainTex.xyz ) * 0.5 + 0.5;
		
		if( _FlipNormalY == 0 ){
			mainTex.y = 1.0 - mainTex.y;
		}
		
		return float4( mainTex.xyz, 1.0 );
	}
	
	float4 fragEdge (v2f IN) : SV_Target
	{

		float2 pixelSize = ( 1.0 / _ImageSize.xy ) * 0.5;
		
		float2 UV = IN.uv;

		half4 mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) );
		
		half4 mainTexX = tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y, 0, 0 ) ) * 2.0 - 1.0;
		half4 mainTexX2 = tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y, 0, 0 ) ) * 2.0 - 1.0;
		
		half4 mainTexY = tex2Dlod(_MainTex, float4( UV.x, UV.y + pixelSize.y, 0, 0 ) ) * 2.0 - 1.0;
		half4 mainTexY2 = tex2Dlod(_MainTex, float4( UV.x, UV.y - pixelSize.y, 0, 0 ) ) * 2.0 - 1.0;
		
		half diffX = ( mainTexX.x - mainTexX2.x ) * _BlurContrast;
		half diffY = ( mainTexY.y - mainTexY2.y ) * _BlurContrast;
		
		if( _FlipNormalY == 0 ){
			diffY *= -1;
		}

		half diff = ( diffX + 0.5 ) * ( diffY + 0.5 ) * 2.0;
		
		return float4( diff.xxx, 1.0 );
	}

	
	float4 fragCombineEdge (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		half4 mainTex = half4( tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur0Weight;
		half4 blurTex1 = half4( tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur1Weight;
		half4 blurTex2 = half4( tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur2Weight;
		half4 blurTex3 = half4( tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur3Weight;
		half4 blurTex4 = half4( tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur4Weight;
		half4 blurTex5 = half4( tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur5Weight;
		half4 blurTex6 = half4( tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur6Weight;
		
		mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
		
		mainTex *= 1.0 / mainTex.w;
		
		if( mainTex.x > 0.5 ){
			mainTex.x = max( mainTex.x * 2.0 - 1.0, 0.0 );
			mainTex.x = pow( mainTex.x, _Pinch );
			mainTex.x *= _EdgeAmount;
			mainTex.x = mainTex.x * 0.5 + 0.5;
		}else{
			mainTex.x = max( -( mainTex.x * 2.0 - 1.0 ), 0.0 );
			mainTex.x = pow( mainTex.x, _Pinch );
			mainTex.x *= _CreviceAmount;
			mainTex.x = -mainTex.x * 0.5 + 0.5;
		}
		
		
		mainTex.x = ( ( mainTex.x - 0.5 ) * _FinalContrast ) + 0.5;
				
		mainTex.x = pow( mainTex.x, _Pillow );
				
		mainTex.x = saturate( mainTex.x + _FinalBias );
		
		return float4( mainTex.xxx, 1.0 );
	}
	
	float rand(float3 co){
		return frac( sin( dot( co.xyz ,float3(12.9898,78.233, 137.9462) ) ) * 43758.5453);
	}
	
	float _Progress;
	
	float4 fragAO (v2f IN) : SV_Target
	{
	
		float2 pixelSize = ( 1.0 / _ImageSize.xy );
		float2 UV = IN.uv;

		float3 flipTex = float3(1,1,1);
		if( _FlipNormalY == 0 ){
			flipTex = float3(1,-1,1);
		}
		
		half3 mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) ).xyz;
		mainTex = normalize( mainTex * 2.0 - 1.0 );
		mainTex *= flipTex;
		
		half mainHeight = tex2Dlod(_HeightTex, float4( UV.xy, 0, 0 ) ).x;
		
		int AOSamples = 50;
		float startOffset = rand( float3( UV, _Time.y ) );
		int i;
		
		float2 AO = 0;
		float AOAccum = 0;
		
		float2 direction;
		direction.x = sin( _Progress * 6.28318530718 );
		direction.y = cos( _Progress * 6.28318530718 );


		float oneOverSpread = 1.0 / (float)_Spread;
		
		for( i = 1; i <= AOSamples; i++ ){
			
			float progress = float(i) / float(AOSamples);
			float2 randomizer = float2( rand( float3( UV.xy, (float)i ) ), rand( float3( UV.yx, (float)i ) ) ) * progress * 0.1;
			float2 uvOffset = direction * _Spread * progress + randomizer;
			float2 trueDir = normalize( uvOffset );

			float2 sampleUV = UV.xy + pixelSize.xy * uvOffset;

			half3 sampleTex = tex2Dlod(_MainTex, float4( sampleUV, 0, 0 ) ).xyz;
			half sampleHeight = tex2Dlod(_HeightTex, float4( sampleUV, 0, 0 ) ).x;
			sampleTex = sampleTex * 2.0 - 1.0;
			sampleTex *= flipTex;

			// Normal Only, same as depth from normal
			float sampleImportance = sqrt( 1.0 - progress );
			AO.x += dot( sampleTex.xyz, trueDir ) * sampleImportance;
			AOAccum += sampleImportance;

			// Depth Only, pure geometric occlusion
			float3 samplePos = float3( trueDir * _Spread * progress, ( sampleHeight.x - mainHeight.x ) * _Depth );
			float sampleDist = saturate( length( samplePos ) * 0.1 );
			float sampleAO = saturate( dot( float3(0,0,1), normalize( samplePos ) ) );
			AO.y = max( sampleAO * sampleDist, AO.y );
		}
		
		AO.x *= 1.0 / AOAccum;
		float AOX1 = saturate( AO.x + 1.0 ); // Dark Parts
		float AOX2 = saturate( AO.x + 0.5 ); // light parts
		AO.x = pow( AOX1, 5.0 );
		AO.x *= pow( AOX2, 0.2 );
		AO.x = sqrt(AO.x);

		AO.y = 1.0 - AO.y;
		
		half2 blendTex = tex2Dlod(_BlendTex, float4( UV.xy, 0, 0 ) ).xy;
		
		AO = lerp( blendTex.xy, AO, _BlendAmount );
		
		return float4( AO.xy, 1.0, 1.0 );
	}
	
	float4 fragCombineAO (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;

		half2 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xy;

		half AO = lerp( mainTex.x, mainTex.y, _AOBlend);
		
		AO += _FinalBias;
		AO = pow( AO, _FinalContrast );
		AO = saturate( AO );
		
		return float4( AO.xxx, 1.0 );
		
	}
	
	float4 fragFixTGA (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;
		
		//Swizzle channels because TGA importer is bonkers
		float3 mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) ).zyx;
		
		return float4( mainTex, 1.0 );
		
	}
	
	float4 fragCombineRoughSpec(v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		half3 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xyz;
				
		// Main tex brightness and contrast
		mainTex = saturate( ( mainTex - 0.5 ) * _DiffuseContrast + 0.5 + _DiffuseBias );
		
		half3 blurTex = tex2Dlod(_BlurTex, float4( UV, 0, 0 ) ).xyz;
		
		// Hot and dark spot removal
		float maxLum = max( max( mainTex.x, mainTex.y ), mainTex.z ); 
		float lightMask = smoothstep( _LightMaskPow, 1.001, maxLum );
		float darkMask = 1.0 - smoothstep( -0.001, _DarkMaskPow, maxLum );
		mainTex = lerp( mainTex, blurTex, 1.0 - ( 1.0 - ( lightMask * _LightPow ) ) * ( 1.0 - ( darkMask * _DarkPow ) ) );
		
		// High / Low pass the diffuse
		blurTex = saturate( ( blurTex - 0.5 ) * _BlurContrast + 0.5 );
		mainTex = BlendVividLightf( mainTex, blurTex );
		
		// Brightness and contrast
		mainTex.xyz = saturate( ( mainTex.xyz - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );
		
		// Desaturate
		mainTex.xyz = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
		
		return float4( mainTex.xyz, 1.0 );
	}
	
	float4 fragEditDiffuse(v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		// Texturel lookups
		half3 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xyz;
		half3 blurTex = tex2Dlod(_BlurTex, float4( UV, 0, 0 ) ).xyz;
		half3 avgColor = tex2Dlod(_AvgTex, float4( UV, 0, 0 ) ).xyz;

		// overlay
		half3 overlay = mainTex - blurTex;

		// save original copy
		half3 maintexInitialHSL = RGBToHSL( mainTex.xyz );

		// spot removal
		//float maxLum = max( max( mainTex.x, mainTex.y ), mainTex.z );
		float avgLum = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
		float lightMask = smoothstep( 1.0 - _HotSpot, ( 1.0 - _HotSpot ) + pow( _HotSpot, 0.5 ) + 0.01, avgLum );
		float darkMask = smoothstep( _DarkSpot - pow( _DarkSpot, 0.5 ) - 0.01, _DarkSpot, avgLum ); // this mask is inverted
		mainTex = lerp( mainTex, avgColor, 1.0 - ( 1.0 - lightMask ) * darkMask );

		// Lighting removal
		float lightMaskPow = saturate( ( _LightMaskPow - 0.5 ) * 2.0 ) + 1.0;
		lightMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _LightMaskPow - 0.5 ) * -2.0 ) + 1.0) );

		float darkMaskPow = saturate( ( _DarkMaskPow - 0.5 ) * 2.0 ) + 1;
		darkMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _DarkMaskPow - 0.5 ) * -2.0 ) + 1.0) );

		mainTex = ( mainTex - avgColor);
		half mainTexGrey = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
		half3 mainTexHighMask = pow( clamp( mainTexGrey * 2.0, 0.001, 0.99 ), lightMaskPow );
		half3 mainTexLowMask = pow( clamp( -mainTexGrey * 2.0, 0.001, 0.99 ), darkMaskPow );
		mainTex += 0.5;

		//mainTex = clamp( mainTex, 0.001, 0.99 );
		//mainTex = lerp( mainTex, pow( mainTex, _LightPow * 5.0 + 1.0 ), mainTexHighMask );
		//mainTex = lerp( mainTex, pow( mainTex, 1.0 / ( _DarkPow * 5.0 + 1.0 ) ), mainTexLowMask );

		mainTex = lerp( mainTex, mainTex * ( 1.0 - _LightPow ) , mainTexHighMask );
		mainTex = lerp( mainTex, 1.0 - ( 1.0 - mainTex ) * ( 1.0 - _DarkPow ), mainTexLowMask );

		float desaturateMask = mainTexHighMask * _LightPow;
		desaturateMask += mainTexLowMask * _DarkPow * 2.0;
		desaturateMask += 1.0 - ( 1.0 - lightMask ) * darkMask;
		desaturateMask = 1.0 - saturate( desaturateMask );

		// apply overlay
		float overlayMask = 1.0 - ( 1.0 - mainTexHighMask * _LightPow ) * ( 1.0 - mainTexLowMask * _DarkPow );
		overlayMask = saturate( overlayMask * 2.0 + 0.1 );
		mainTex *= lerp( 1.0, overlay * _BlurContrast * 10.0 + 1.0, overlayMask );
		mainTex = saturate( mainTex );

		// maintain color
		half3 maintexHSL = RGBToHSL( saturate( mainTex.xyz ) );
		maintexHSL.xy = maintexInitialHSL.xy;
		float3 mainTexOriginalColor = HSLToRGB( maintexHSL );
		mainTex.xyz = lerp( mainTex.xyz, mainTexOriginalColor.xyz, _ColorLerp * desaturateMask );

		// brightness and contrast
		mainTex = saturate( ( mainTex - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );

		// Saturation
		mainTex.xyz = lerp( mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2, mainTex.xyz, _Saturation );
		
		return float4( saturate( mainTex.xyz ), 1.0 );
	}
		
	
	ENDCG
		
	SubShader {
	
		// Desaturate Pass 0
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragDesaturate
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		// Blur Pass 1
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlur
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		// Combine Height Pass 2
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombineHeight
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Normal from Height Pass 3
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragNormal
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Combine Normal Pass 4
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombineNormal
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Edge Pass 5
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragEdge
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Combine Edge Pass 6
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombineEdge
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		// AO Pass 7
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragAO
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Combine AO Pass 8
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombineAO
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Fix TGA 9
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragFixTGA
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		// Combine Spec Roughness 10
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombineRoughSpec
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		// Edit Diffuse 11
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragEditDiffuse
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
