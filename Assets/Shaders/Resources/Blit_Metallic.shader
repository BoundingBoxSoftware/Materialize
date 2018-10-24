// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Metallic" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
		
	float4 _MetalColor;
	
	float2 _SampleUV;
	
	float _HueWeight;
	float _SatWeight;
	float _LumWeight;
	
	float _MaskLow;
	float _MaskHigh;
	
	float _DiffuseContrast;
	float _DiffuseBias;
	
	sampler2D _BlurTex;
	
	sampler2D _OverlayBlurTex;
	float _BlurOverlay;

	float _FinalContrast;
	float _FinalBias;
	
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

	float4 fragMetallic (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;

		half3 mainTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) ).xyz;
		half3 blurTex = tex2Dlod( _BlurTex, float4( UV, 0, 0 ) ).xyz;
		half3 overlayBlurTex = tex2Dlod( _OverlayBlurTex, float4( UV, 0, 0 ) ).xyz;
		
		half3 overlay = ( mainTex - overlayBlurTex );
		half overlayGrey = overlay.x * 0.3 + overlay.y * 0.5 + overlay.z * 0.2;
		
		half3 metalSample = tex2Dlod( _BlurTex, float4( _SampleUV, 0, 0 ) ).xyz;
		
		half3 mainTexHSL = RGBToHSL( mainTex.xyz );
		half3 blurTexHSL = RGBToHSL( blurTex.xyz );
		half3 metalHSL = RGBToHSL( _MetalColor.xyz );
		half3 metalSampleHSL = metalHSL;//RGBToHSL( metalSample.xyz );
		
		float hueDif = 1.0 - min( min( abs( blurTexHSL.x - metalSampleHSL.x ), abs( ( blurTexHSL.x + 1.0 ) - metalSampleHSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - metalSampleHSL.x ) ) * 2.0;
		float satDif = 1.0 - abs( blurTexHSL.y - metalSampleHSL.y );
		float lumDif = 1.0 - abs( blurTexHSL.z - metalSampleHSL.z );
		
		float finalDiff = ( hueDif * _HueWeight ) + ( satDif * _SatWeight ) + ( lumDif * _LumWeight );
		finalDiff *= 1.0 / ( _HueWeight + _SatWeight + _LumWeight );
		finalDiff = smoothstep( _MaskLow, _MaskHigh, finalDiff );
		
		finalDiff = saturate( ( finalDiff - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );
		finalDiff *= clamp( ( overlayGrey * _BlurOverlay ) + 1.0, 0.0, 10.0 );
		finalDiff = saturate( finalDiff );

		return float4( saturate( finalDiff.xxx ), 1.0 );

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
			#pragma fragment fragMetallic
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
