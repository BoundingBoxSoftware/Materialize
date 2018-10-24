Shader "Custom/Seamless_Texture_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_HeightTex ("Height", 2D) = "white" {}
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
			sampler2D _HeightTex;
			
			float _Falloff;
			float _OverlapX;
			float _OverlapY;
			float _SmoothBlend;
			float _GamaCorrection;

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {

				float2 overlap = float2( _OverlapX, _OverlapY );
				float2 invOverlap = 1.0 - float2( _OverlapX, _OverlapY );
				float2 oneOverOverlap = 1.0 / float2( _OverlapX, _OverlapY );
				
				float2 UV = IN.uv;
				float2 UV2 = UV - float2( overlap.x, 0.0 );
				float2 UV3 = UV - float2( 0.0, overlap.y );
				float2 UV4 = UV - float2( overlap.x, overlap.y );
				
				float2 UVMask = saturate( (  1.0 - frac( UV + 0.0 ) - invOverlap ) * oneOverOverlap );
				
				UV = frac( UV );
				UV2 = frac( UV2 );
				UV3 = frac( UV3 );
				UV4 = frac( UV4 );
				
				UV *= invOverlap;
				
				UV2.x += overlap.x;
				UV2 *= invOverlap;
				
				UV3.y += overlap.y;
				UV3 *= invOverlap;
				
				UV4 += overlap;
				UV4 *= invOverlap;
			
				half heightTex = tex2Dlod(_HeightTex, float4( UV, 0, 0 ) ).x;
				half heightTex2 = tex2Dlod(_HeightTex, float4( UV2, 0, 0 ) ).x;
				half heightTex3 = tex2Dlod(_HeightTex, float4( UV3, 0, 0 ) ).x;
				half heightTex4 = tex2Dlod(_HeightTex, float4( UV4, 0, 0 ) ).x;
				
				half4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
				half4 mainTex2 = tex2Dlod(_MainTex, float4( UV2, 0, 0 ) );
				half4 mainTex3 = tex2Dlod(_MainTex, float4( UV3, 0, 0 ) );
				half4 mainTex4 = tex2Dlod(_MainTex, float4( UV4, 0, 0 ) );

				half SSHigh =  0.01 + ( 0.5 * saturate( _Falloff ) );
				half SSLow =  -0.01 - ( 0.5 * saturate( _Falloff ) );
				half TexBlend = smoothstep( SSLow, SSHigh, ( heightTex2 + UVMask.x ) - ( heightTex + ( 1.0 - UVMask.x ) ) );
				
				half4 blendTexH = lerp( mainTex, mainTex2, UVMask.x );
				half heightTexH = lerp( heightTex, heightTex2, TexBlend );
				half4 mainTexH = lerp( mainTex, mainTex2, TexBlend );
				
				TexBlend = smoothstep( SSLow, SSHigh, ( heightTex4 + UVMask.x ) - ( heightTex3 + ( 1.0 - UVMask.x ) ) );
				
				half4 blendTexV = lerp( mainTex3, mainTex4, UVMask.x );
				half heightTexV = lerp( heightTex3, heightTex4, TexBlend );
				half4 mainTexV = lerp( mainTex3, mainTex4, TexBlend );
				
				TexBlend = smoothstep( SSLow, SSHigh, ( heightTexV + UVMask.y ) - ( heightTexH + ( 1.0 - UVMask.y ) ) );
				
				half4 blendTex = lerp( mainTexH, mainTexV, UVMask.y );
				heightTex = lerp( heightTexH, heightTexV, TexBlend );
				mainTex = lerp( mainTexH, mainTexV, TexBlend );	
				
				if( _SmoothBlend > 1.0 ){
					mainTex = blendTex;
				}	
			
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
				surfOut.Emission = pow( mainTex, _GamaCorrection );
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
