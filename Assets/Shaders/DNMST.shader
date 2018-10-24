Shader "Custom/DNMST" {
	Properties {
		_DiffuseMap("Diffuse", 2D) = "grey" {}
		_NormalMap("Normal", 2D) = "bump" {}
		_SmoothnessMap("Smoothness", 2D) = "black" {}
		_MetallicMap("Metallic", 2D) = "black" {}
		_AOMap("Ambient Occlusion", 2D) = "white" {}
		_EdgeMap("Edge", 2D) = "grey" {}
		_DisplacementMap("Displacement", 2D) = "grey" {}
		
		_Tint("Diffuse Tint", Color) = (0.5,0.5,0.5,1)
		
		_AOPower ("AO Power", Float ) = 1.0
		_EdgePower ("Edge Power", Float ) = 1.0

		_Smoothness ("Smoothness", Float ) = 1.0
		_Metallic ("Metallic", Float ) = 1.0
		_Dust ("Dust", Float ) = 0.0
		
		_Parallax ("Height", Range (0.0, 3.0)) = 0.5
		_EdgeLength ("Edge length", Range(3,50)) = 3
		
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
			sampler2D _SmoothnessMap;
			sampler2D _MetallicMap;
			sampler2D _AOMap;
			sampler2D _EdgeMap;
			sampler2D _DisplacementMap;
			
			float4 _Tiling;
			
			float4 _Tint;
			float _AOPower;
			float _EdgePower;
			float _Smoothness;
			float _Metallic;
			float _Dust;
			
			float _GamaCorrection;
			
			int _FlipNormalY;

			float _Parallax;
					
			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
				// Albedo comes from a texture tinted by color
				
				float2 UV = IN.uv.xy * _Tiling.xy + _Tiling.zw;

				float2 uv_dx = ddx( UV );
				float2 uv_dy = ddy( UV );

				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );

				fixed3 localViewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y  + IN.tSpace2.xyz * worldViewDir.z;
				localViewDir = normalize( localViewDir );

				int steps = 50;
				int subSteps = 10;
				float ssf = 1.0 / (float)subSteps;
				half dispFactor = _Parallax;
				float2 dispDir = localViewDir.xy * ( 1.0 / localViewDir.z );
				int i = 0;
				int j = 0;

				for( i = 0; i < steps; i++ ){
					float2 dUV = UV - dispDir * i * dispFactor * 0.002;
					half d = ( 1.0 - tex2D(_DisplacementMap, dUV, uv_dx, uv_dy).x ) * (float)steps;

					if( d <= (float)i ){
						UV = dUV;
						break;
					}
				}

				for( j = 1; j <= ( subSteps + 1 ); j++ ){
					float2 dUV = UV + dispDir * j * dispFactor * 0.002 * ssf;
					half d = ( 1.0 - tex2D(_DisplacementMap, dUV, uv_dx, uv_dy).x ) * (float)steps;
					if( d >= ( (float)i - (float)j * ssf ) ){
						UV = dUV;
						break;
					}
				}

				half3 texDiffuse = tex2D (_DiffuseMap, UV, uv_dx, uv_dy).xyz;
				half3 texNormal = tex2D(_NormalMap,UV, uv_dx, uv_dy).xyz;
				half3 texMetallic = tex2D(_MetallicMap,UV, uv_dx, uv_dy).xyz;
				half texSmoothness = tex2D(_SmoothnessMap,UV, uv_dx, uv_dy).x;
				half texAO = tex2D(_AOMap,UV, uv_dx, uv_dy).x;
				half texEdge = tex2D(_EdgeMap,UV, uv_dx, uv_dy).x;
				
				texNormal.xyz = texNormal.xyz * 2.0 - 1.0;
				if( _FlipNormalY == 0 ){
					texNormal.y *= -1.0;
				}
				
				// Deferred stuff
				//float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				//fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				
				fixed3 worldNormal;
				worldNormal.x = dot(IN.tSpace0.xyz, texNormal.xyz);
				worldNormal.y = dot(IN.tSpace1.xyz, texNormal.xyz);
				worldNormal.z = dot(IN.tSpace2.xyz, texNormal.xyz);
				worldNormal = normalize( worldNormal );
				
				float fresnel = 1.0 - saturate( dot( worldViewDir, worldNormal ) * 0.5 + 0.5 );
				float schlickFresnel = pow( fresnel, 3.0 );
				
				texDiffuse.xyz = pow( texDiffuse.xyz, 1.0 - saturate( saturate( _Dust ) * fresnel ) );
				texDiffuse.xyz *= ( texEdge - 0.5 ) * _EdgePower + 1.0;
				
				texAO *= texEdge + 0.5;
				texDiffuse.xyz = saturate( texDiffuse.xyz );
				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
				surfOut.Normal = worldNormal.xyz;
				surfOut.Metallic = saturate( _Metallic * texMetallic.x );
				surfOut.Smoothness = saturate( lerp( saturate( _Smoothness * texSmoothness ), 1.0, schlickFresnel ) );
				surfOut.Transmission = 0.0;
				surfOut.Emission = 0.0;
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = saturate( lerp( 1.0, texAO, _AOPower ) );
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );
				
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
