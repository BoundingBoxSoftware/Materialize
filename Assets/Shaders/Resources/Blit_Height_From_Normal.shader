// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Height_From_Normal" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "Photoshop.cginc"
	
	float4 _ImageSize;
	float _Spread;
	int _Samples;
	
	float _FinalContrast;
	float _FinalBias;
	
	sampler2D _MainTex;
	//sampler2D _HeightTex;
	sampler2D _BlendTex;
	
	float _BlendAmount;
	float _Progress;
	float _SpreadBoost;
	
	int _FlipNormalY;
	
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
	
	float rand(float3 co){
		return frac( sin( dot( co.xyz ,float3(12.9898,78.233, 137.9462) ) ) * 43758.5453);
	}
	
	float4 fragHeight (v2f IN) : SV_Target
	{
	
		float2 pixelSize = ( 1.0 / _ImageSize.xy );
		float2 UV = IN.uv;

		float3 flipTex = float3(1,1,1);
		if( _FlipNormalY == 0 ){
			flipTex = float3(1,-1,1);
		}
		
		//half3 mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) ).xyz;
		//mainTex = mainTex * 2.0 - 1.0;
		//mainTex *= flipTex;
		
		int AOSamples = _Samples;
		float startOffset = rand( float3( UV, _Time.y ) );
		int i;
		
		float2 direction;
		direction.x = sin( _Progress * 6.28318530718 );
		direction.y = cos( _Progress * 6.28318530718 );
		
		float Weight = 0.0;
		float TotalWeight = 0.0;
		float AO = 0.0;
		for( i = 1; i <= AOSamples; i++ ){
			
			float passProgress = float(i) / float(AOSamples);
			float2 randomizer = float2( rand( float3( UV.xy, (float)i ) ), rand( float3( UV.yx, (float)i ) ) ) * passProgress * 0.1;
			float2 uvOffset = direction * _Spread * ( passProgress * _SpreadBoost ) + randomizer;
			float2 trueDir = normalize( uvOffset );

			float2 sampleUV = UV.xy + pixelSize.xy * uvOffset;

			float3 sampleTex = tex2Dlod(_MainTex, float4( sampleUV, 0, 0 ) ).xyz;
			sampleTex = sampleTex * 2.0 - 1.0;
			sampleTex *= flipTex;
			
			float sampleAO = dot( sampleTex, normalize( float3( trueDir, 0.0 ) ) );
			
			//Weight = sqrt( 1.0 - passProgress );
			//Weight = 1.0 - passProgress;
			Weight = 1.0;
			//Weight = 3.14 * passProgress;
			TotalWeight += Weight;
			AO += sampleAO * Weight;
			
		}

		AO *= 1.0 / TotalWeight;
		AO *= ( (float)AOSamples * _SpreadBoost ) / 50.0;
		AO = AO * 0.5 + 0.5;


		
		float blendTex = tex2Dlod(_BlendTex, float4( UV.xy, 0, 0 ) ).x;
		
		AO = lerp( blendTex.x, AO, _BlendAmount );
		
		return float4( AO.xxx, 1.0 );
	}
	
	float4 fragCombineHeight (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;
		
		half mainTex = tex2Dlod(_MainTex, float4( UV.xy, 0, 0 ) ).x;
		
		mainTex = ( mainTex - 0.5 ) * _FinalContrast + 0.5;
		mainTex += _FinalBias;
		mainTex = saturate( mainTex );
		
		return float4( mainTex.xxx, 1.0 );
		
	}
	
	ENDCG
		
	SubShader {
	
		// Height From Normal Pass 0
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragHeight
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		// Combine Height From Normal Pass 1
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
	}
	Fallback off
}
