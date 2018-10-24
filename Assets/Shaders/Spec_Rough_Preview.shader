Shader "Custom/Spec_Rough_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass
		{
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" }
			Fog {Mode Off}
			
			CGPROGRAM
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma target 3.0
			
			#pragma exclude_renderers nomrt
			#pragma multi_compile_prepassfinal
			#define UNITY_PASS_DEFERRED
			
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			sampler2D _MainTex;
			float _DiffuseContrast;
			float _DiffuseBias;
			
			sampler2D _BlurTex;
			float _BlurWeight;
			float _BlurContrast;
			
			float _LightMaskPow;
			float _LightPow;
			
			float _DarkMaskPow;
			float _DarkPow;

			float _FinalContrast;
			float _FinalBias;
			
			float _ColorLerp;
			
			float _Saturation;
			
			float _GamaCorrection;
			
			#include "Photoshop.cginc"

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {

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
				
				
				//Deferred Stuff
				
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				
				fixed3 localNormal = float3(0,0,1);
				
				fixed3 worldNormal;
				worldNormal.x = dot(IN.tSpace0.xyz, localNormal);
				worldNormal.y = dot(IN.tSpace1.xyz, localNormal);
				worldNormal.z = dot(IN.tSpace2.xyz, localNormal);
				worldNormal = normalize( worldNormal );
				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = float3(0,0,0);
				surfOut.Normal = worldNormal.xyz;
				surfOut.Metallic = 0;
				surfOut.Smoothness = 0;
				surfOut.Transmission = 0;
				surfOut.Emission = saturate( pow( mainTex.xyz, _GamaCorrection ) );
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = 0;
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );
				
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
