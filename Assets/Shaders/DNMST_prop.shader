Shader "Custom/DNMST_prop" {
	Properties {
		_DiffuseMap("Diffuse", 2D) = "grey" {}
		_NormalMap("Normal", 2D) = "bump" {}
		_PropertyMap("Property", 2D) = "black" {}
		
		_Tint("Diffuse Tint", Color) = (0.5,0.5,0.5,1)
		
		_AOPower ("AO Power", Float ) = 1.0
		_Smoothness ("Smoothness", Float ) = 1.0
		_Metallic ("Metallic", Float ) = 1.0
		
		_Tiling ("Tiling", Vector ) = (1.0,1.0,0.0,0.0)
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
			
			sampler2D _DiffuseMap;
			sampler2D _NormalMap;
			sampler2D _PropertyMap;
			
			float4 _Tiling;
			
			float4 _Tint;
			float _AOPower;
			float _EdgePower;
			float _Smoothness;
			float _Metallic;
			float _Dust;
			
			float _GamaCorrection;
			
			int _FlipNormalY;
					
			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
				// Albedo comes from a texture tinted by color
				
				float2 UV = IN.uv.xy * _Tiling.xy + _Tiling.zw;
			
				half3 texDiffuse = tex2D (_DiffuseMap, UV).xyz;
				half3 texNormal = tex2D(_NormalMap,UV).xyz;
				half3 texProperty = tex2D(_PropertyMap,UV).xyz;

				
				texNormal.xyz = texNormal.xyz * 2.0 - 1.0;
				if( _FlipNormalY == 0 ){
					texNormal.y *= -1.0;
				}
				
				// Deferred stuff
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				
				fixed3 worldNormal;
				worldNormal.x = dot(IN.tSpace0.xyz, texNormal.xyz);
				worldNormal.y = dot(IN.tSpace1.xyz, texNormal.xyz);
				worldNormal.z = dot(IN.tSpace2.xyz, texNormal.xyz);
				worldNormal = normalize( worldNormal );
				
				float fresnel = 1.0 - saturate( dot( worldViewDir, worldNormal ) * 0.5 + 0.5 );
				float schlickFresnel = pow( fresnel, 3.0 );
				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
				surfOut.Normal = worldNormal.xyz;
				surfOut.Metallic = saturate( _Metallic * texProperty.x );
				surfOut.Smoothness = saturate( _Smoothness * texProperty.y );
				surfOut.Transmission = 0.0;
				surfOut.Emission = 0.0;
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = saturate( lerp( 1.0, texProperty.z, _AOPower ) );

				ReturnOutput(surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission);
				
			}
				
			ENDCG
		}
		
		
	 	Pass
		{
			Name "META" 
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2

			#include "UnityStandardMeta.cginc"
			ENDCG
		} 
	}
	FallBack "Diffuse"
}
