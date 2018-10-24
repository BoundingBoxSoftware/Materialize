Shader "Custom/CubeMapBackground" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CubeMap ("Cube Map", CUBE) = "" {}
		_Blur( "Blur" , Float ) = 2.0
		_Factor( "Factor" , Float ) = 1.0
	}
	SubShader {
		Tags { "RenderQueue"="Opaque" "RenderType"="Opaque" }
		LOD 200
		Cull Front
		
		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			samplerCUBE _CubeMap;
			float _Blur;
			float _Factor;
			
			uniform samplerCUBE _GlobalCubemap;
			uniform samplerCUBE _ProbeCubemap;

			// vertex-to-fragment interpolation data
			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
			};

			// vertex shader
			v2f vert (appdata_full v) {
				v2f o;
				o.pos = UnityObjectToClipPos ( v.vertex );
				o.worldNormal = UnityObjectToWorldDir(v.normal.xyz);
				return o;
			}

			static const int BlurKernelSamples = 27;		
			static const float3 BlurKernel[BlurKernelSamples] =
			{
				float3(1,1,1),
				float3(0,1,1),
				float3(-1,1,1),

				float3(1,0,1),
				float3(0,0,1),
				float3(-1,0,1),

				float3(1,-1,1),
				float3(0,-1,1),
				float3(-1,-1,1),

				//====================//

				float3(1,1,0),
				float3(0,1,0),
				float3(-1,1,0),

				float3(1,0,0),
				float3(0,0,0),
				float3(-1,0,0),

				float3(1,-1,0),
				float3(0,-1,0),
				float3(-1,-1,0),

				//====================//

				float3(1,1,-1),
				float3(0,1,-1),
				float3(-1,1,-1),

				float3(1,0,-1),
				float3(0,0,-1),
				float3(-1,0,-1),

				float3(1,-1,-1),
				float3(0,-1,-1),
				float3(-1,-1,-1)
			};
			
			// fragment shader
			fixed4 frag (v2f IN) : SV_Target {
				
				fixed3 worldNormal = normalize( IN.worldNormal.xyz );

				float3 ambIBL = 0.0;
				for( int i = 0; i < BlurKernelSamples; i++ ){
					ambIBL += texCUBElod(_ProbeCubemap, half4( worldNormal + BlurKernel[i] * 0.025 , 1.0 ) ).xyz;
				}
				ambIBL *= 1.0 / BlurKernelSamples;

				//ambIBL = texCUBElod(_ProbeCubemap, half4( worldNormal, 1.0 ) ).xyz;
				
				//return float4( ( ambIBL + ( ambIBL * ambIBL ) ) * _Factor, 1.0 );
				return float4( ambIBL * _Factor,1.0 );
				
			}
			ENDCG
		}
		
		/*
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
			#pragma multi_compile _USE_BAKED_CUBEMAP_ON _USE_BAKED_CUBEMAP_OFF
			#define UNITY_PASS_DEFERRED
			
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
		
			sampler2D _MainTex;
			samplerCUBE _CubeMap;
			float _Blur;
			float _Factor;

			float _UseProbeTexture;
			
			uniform samplerCUBE _GlobalCubemap;
			uniform samplerCUBE _ProbeCubemap;
			
			#include "DNMST.cginc"

			static const int BlurKernelSamples = 27;		
			static const float3 BlurKernel[BlurKernelSamples] =
			{
				float3(1,1,1),
				float3(0,1,1),
				float3(-1,1,1),

				float3(1,0,1),
				float3(0,0,1),
				float3(-1,0,1),

				float3(1,-1,1),
				float3(0,-1,1),
				float3(-1,-1,1),

				//====================//

				float3(1,1,0),
				float3(0,1,0),
				float3(-1,1,0),

				float3(1,0,0),
				float3(0,0,0),
				float3(-1,0,0),

				float3(1,-1,0),
				float3(0,-1,0),
				float3(-1,-1,0),

				//====================//

				float3(1,1,-1),
				float3(0,1,-1),
				float3(-1,1,-1),

				float3(1,0,-1),
				float3(0,0,-1),
				float3(-1,0,-1),

				float3(1,-1,-1),
				float3(0,-1,-1),
				float3(-1,-1,-1)
			};
			
			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
				// Albedo comes from a texture tinted by color
				
				fixed3 localNormal = float3(0,0,1);

				fixed3 worldNormalBaked = normalize( IN.localNormal.xyz );
				fixed3 worldNormal = normalize( float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z ) );
				
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );


				float3 ambIBL = 0.0;
				for( int i = 0; i < BlurKernelSamples; i++ ){
					ambIBL += texCUBElod(_ProbeCubemap, half4( worldNormal + BlurKernel[i] * 0.025 , 1.0 ) ).xyz;
				}
				ambIBL *= 1.0 / BlurKernelSamples;

				#if _USE_BAKED_CUBEMAP_ON
					float3 ambIBLbaked = 0.0;
					for( int i = 0; i < BlurKernelSamples; i++ ){
						ambIBLbaked += texCUBElod(_GlobalCubemap, half4( worldNormalBaked + BlurKernel[i] * 0.025 , 3.0 ) ).xyz;
					}
					ambIBLbaked *= 1.0 / BlurKernelSamples;

					ambIBL = lerp( ambIBLbaked, ambIBL, _UseProbeTexture );
				#endif

				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = float3(0,0,0);
				surfOut.Normal = worldNormal;
				surfOut.Metallic = 0;
				surfOut.Smoothness = 0;
				surfOut.Transmission = 0;
				surfOut.Emission =  ambIBL * _Factor;
				surfOut.Motion = float2(1,1);//float2(0.5,0.5);
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = 0;
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );
				
			}
			ENDCG
		} 
		*/
	}
	FallBack "Diffuse"
}
