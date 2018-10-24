// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Property_Comp" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _RedTex;
	sampler2D _RedOverlayTex;
	sampler2D _GreenTex;
	sampler2D _GreenOverlayTex;
	sampler2D _BlueTex;
	sampler2D _BlueOverlayTex;
	
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

	float4 fragMSAOComp (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;

		half redTex = tex2Dlod( _RedTex, float4( UV, 0, 0 ) ).x;
		half redOverlayTex = tex2Dlod( _RedOverlayTex, float4( UV, 0, 0 ) ).x;
		
		half greenTex = tex2Dlod( _GreenTex, float4( UV, 0, 0 ) ).x;
		half greenOverlayTex = tex2Dlod( _GreenOverlayTex, float4( UV, 0, 0 ) ).x;
		
		half blueTex = tex2Dlod( _BlueTex, float4( UV, 0, 0 ) ).x;
		half blueOverlayTex = tex2Dlod( _BlueOverlayTex, float4( UV, 0, 0 ) ).x;
		
		redTex *= redOverlayTex + 0.5;
		greenTex *= greenOverlayTex + 0.5;
		blueTex *= blueOverlayTex + 0.5;

		return float4( redTex, greenTex, blueTex, 1.0 );

	}	
	
	ENDCG
		
	SubShader {
	
		// MSAO Comp Pass
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragMSAOComp
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
