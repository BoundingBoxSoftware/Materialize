// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Normal_From_Height" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	
	float _BlurScale;
	float4 _ImageSize;
	float _GamaCorrection;
	
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

	float _FinalContrast;
	
	float _IsColor;
	
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
	
	float4 fragNormal (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;
		float2 pixelSize = 1.0 / _ImageSize.xy;
		float4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
		float4 mainTexDDX = tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y, 0, 0 ) );
		float4 mainTexDDY = tex2Dlod(_MainTex, float4( UV.x, UV.y + pixelSize.y, 0, 0 ) );
		
		mainTexDDX = mainTexDDX - mainTex;
		mainTexDDY = mainTexDDY - mainTex;
		
		float3 normalTex = normalize( cross( normalize( float3( 1.0, 0.0, mainTexDDX.x * _Blur0Contrast ) ), normalize( float3( 0.0, 1.0, mainTexDDY.x * _Blur0Contrast ) ) ) );
		
		normalTex = normalTex * 0.5 + 0.5;
		
		return float4( normalTex.xyz, 1.0 );
	
	}
	
	float4 fragCombine (v2f IN) : SV_Target
	{
		float2 UV = IN.uv;

		half4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
		half4 blurTex1 = tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) );
		half4 blurTex2 = tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) );
		half4 blurTex3 = tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) );
		half4 blurTex4 = tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) );
		half4 blurTex5 = tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) );
		half4 blurTex6 = tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) );
		
		mainTex.w = 1.0;
		blurTex1.w = 1.0;
		blurTex2.w = 1.0;
		blurTex3.w = 1.0;
		blurTex4.w = 1.0;
		blurTex5.w = 1.0;
		blurTex6.w = 1.0;
		
		//Put these on slider?
		
		mainTex *= _Blur0Weight;
		blurTex1 *= _Blur1Weight;
		blurTex2 *= _Blur2Weight;
		blurTex3 *= _Blur3Weight;
		blurTex4 *= _Blur4Weight;
		blurTex5 *= _Blur5Weight;
		blurTex6 *= _Blur6Weight;
		
		mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
		
		mainTex *= 1.0 / mainTex.w;
		
		mainTex.xyz = saturate( ( ( mainTex.xyz - 0.5 ) * _FinalContrast ) + 0.5 );
		
		mainTex.xyz = normalize( mainTex.xyz * 2.0 - 1.0 ) * 0.5 + 0.5;
		
		if( _FlipNormalY == 0 ){
			mainTex.y = 1.0 - mainTex.y;
		}
		
		return float4( mainTex.xyz, 1.0 );
	}
	
	float4 fragBlur (v2f IN) : SV_Target
	{

		float2 pixelSize = ( 1.0 / _ImageSize.xy ) * _BlurScale * 0.5;
		
		float2 UV = IN.uv;

		float4 mainTex = tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y - pixelSize.y * 2, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x, UV.y - pixelSize.y * 2, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y - pixelSize.y * 2, 0, 0 ) );
		
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y - pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y - pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x, UV.y - pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y - pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y - pixelSize.y, 0, 0 ) );
		
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x, UV.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y, 0, 0 ) );
		
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y + pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y + pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x, UV.y - pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y + pixelSize.y, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x * 2, UV.y + pixelSize.y, 0, 0 ) );
		
		mainTex += tex2Dlod(_MainTex, float4( UV.x - pixelSize.x, UV.y + pixelSize.y * 2, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x, UV.y + pixelSize.y * 2, 0, 0 ) );
		mainTex += tex2Dlod(_MainTex, float4( UV.x + pixelSize.x, UV.y + pixelSize.y * 2, 0, 0 ) );
		
		mainTex *= 1.0 / 21.0;
		
		mainTex.xyz = saturate( ( ( mainTex.xyz - 0.5 ) * _BlurContrast ) + 0.5 );
		
		return float4( mainTex.xyz, 1.0 );
	}
	
	ENDCG
		
	SubShader {
	
		// Normal Pass
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
		
		// Blur Pass
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
		
		
		// Combine Pass
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCombine
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
