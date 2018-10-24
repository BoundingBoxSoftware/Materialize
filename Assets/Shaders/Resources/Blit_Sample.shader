// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Sample" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _BlurTex;
	sampler2D _MetallicTex;
	
	float4 _SampleColor1;
	float2 _SampleUV1;

	float4 _SampleColor2;
	float2 _SampleUV2;

	float4 _SampleColor3;
	float2 _SampleUV3;
	
	int _UseSample1;
	int _IsolateSample1;
	float _HueWeight1;
	float _SatWeight1;
	float _LumWeight1;
	float _MaskLow1;
	float _MaskHigh1;
	float _Sample1Height;

	int _UseSample2;
	int _IsolateSample2;
	float _HueWeight2;
	float _SatWeight2;
	float _LumWeight2;
	float _MaskLow2;
	float _MaskHigh2;
	float _Sample2Height;

	int _UseSample3;
	int _IsolateSample3;
	float _HueWeight3;
	float _SatWeight3;
	float _LumWeight3;
	float _MaskLow3;
	float _MaskHigh3;
	float _Sample3Height;
	
	float _SampleBlend;
	
	float _BlurOverlay;

	float _FinalContrast;

	float _FinalBias;
	
	float _Slider;
	
	float _GamaCorrection;
	
	
	
	#include "Photoshop.cginc"

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

	float4 fragSample (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;

		half3 mainTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) ).xyz;
		
		half mainTexGrey = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
		
		//half metalMask = tex2Dlod( _MetallicTex, float4( UV, 0, 0 ) ).x;
		
		half3 mainTexHSL = RGBToHSL( mainTex.xyz );
		
		//half3 blurTexHSL = RGBToHSL( blurTex.xyz );
		half3 sampleTexHSL = mainTexHSL;
		
		float sample1Mask = 0.0;
		if( _UseSample1 ){
			half3 sample1HSL = RGBToHSL( _SampleColor1.xyz );
			float sample1HueDif = 1.0 - min( min( abs( sampleTexHSL.x - sample1HSL.x ), abs( ( sampleTexHSL.x + 1.0 ) - sample1HSL.x ) ), abs( ( sampleTexHSL.x - 1.0 ) - sample1HSL.x ) ) * 2.0;
			float sample1SatDif = 1.0 - abs( sampleTexHSL.y - sample1HSL.y );
			float sample1LumDif = 1.0 - abs( sampleTexHSL.z - sample1HSL.z );
			sample1Mask = ( sample1HueDif * _HueWeight1 ) + ( sample1SatDif * _SatWeight1 ) + ( sample1LumDif * _LumWeight1 );
			sample1Mask *= 1.0 / ( _HueWeight1 + _SatWeight1 + _LumWeight1 );
			sample1Mask = smoothstep( _MaskLow1, _MaskHigh1, sample1Mask );
		}
		
		float sample2Mask = 0.0;
		if( _UseSample2 ){
			half3 sample2HSL = RGBToHSL( _SampleColor2.xyz );
			float sample2HueDif = 1.0 - min( min( abs( sampleTexHSL.x - sample2HSL.x ), abs( ( sampleTexHSL.x + 1.0 ) - sample2HSL.x ) ), abs( ( sampleTexHSL.x - 1.0 ) - sample2HSL.x ) ) * 2.0;
			float sample2SatDif = 1.0 - abs( sampleTexHSL.y - sample2HSL.y );
			float sample2LumDif = 1.0 - abs( sampleTexHSL.z - sample2HSL.z );
			sample2Mask = ( sample2HueDif * _HueWeight2 ) + ( sample2SatDif * _SatWeight2 ) + ( sample2LumDif * _LumWeight2 );
			sample2Mask *= 1.0 / ( _HueWeight2 + _SatWeight2 + _LumWeight2 );
			sample2Mask = smoothstep( _MaskLow2, _MaskHigh2, sample2Mask );
		}
		
		float finalHeight = mainTexGrey;
		
		if( _UseSample2 ){
			finalHeight = lerp( finalHeight, _Sample2Height, sample2Mask * _SampleBlend );
		}
		if( _UseSample1 ){
			finalHeight = lerp( finalHeight, _Sample1Height, sample1Mask * _SampleBlend );
		}

		//finalHeight = saturate( ( finalHeight - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );
		finalHeight = saturate( finalHeight );
		
		if( _IsolateSample1 == 1 ){
			finalHeight = sample1Mask;
		}
		
		if( _IsolateSample2 == 1 ){
			finalHeight = sample2Mask;
		}

		return float4( saturate( pow( finalHeight.xxx, _GamaCorrection ) ), 1.0 );

	}	
	
	ENDCG
		
	SubShader {
	
		// Metallic Pass
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragSample
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
