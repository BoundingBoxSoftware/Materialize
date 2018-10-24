// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_FlipNormalY" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	
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

	float4 frag (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;

		float4 normalTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) );
		normalTex.y = 1.0 - normalTex.y;
		normalTex.w = 1.0;

		return normalTex;

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
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
