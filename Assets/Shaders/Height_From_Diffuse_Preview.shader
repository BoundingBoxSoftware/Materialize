Shader "Custom/Height_From_Diffuse_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex0 ("Base (RGB)", 2D) = "white" {}
		_BlurTex1 ("Base (RGB)", 2D) = "white" {}
		_BlurTex2 ("Base (RGB)", 2D) = "white" {}
		_BlurTex3 ("Base (RGB)", 2D) = "white" {}
		_BlurTex4 ("Base (RGB)", 2D) = "white" {}
		_BlurTex5 ("Base (RGB)", 2D) = "white" {}
		_BlurTex6 ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200


		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			
			sampler2D _BlurTex0;
			sampler2D _BlurTex1;
			sampler2D _BlurTex2;
			sampler2D _BlurTex3;
			sampler2D _BlurTex4;
			sampler2D _BlurTex5;
			sampler2D _BlurTex6;

			sampler2D _AvgTex;
			
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
			float _FinalBias;
			
			float _GamaCorrection;
			
			float _FinalGain;
			
			float _Slider;

			float _IsNormal;
			
			bool _Isolate;

			// vertex-to-fragment interpolation data
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// vertex shader
			v2f vert (appdata_full v) {
				v2f o;
				o.pos = UnityObjectToClipPos ( v.vertex );
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target {

				float2 UV = IN.uv;

				float4 heightTex = float4(0,0,0,0);

				half4 mainTex = tex2Dlod(_MainTex, float4(UV, 0, 0));
				half noBlurTex = 0;
				
				if (_IsNormal > 0.5) {
					heightTex = tex2Dlod(_BlurTex0, float4(UV, 0, 0)).xxxx;
				} else {
					float totalContrast = 7.0 / (_Blur0Contrast + _Blur1Contrast + _Blur2Contrast + _Blur3Contrast + _Blur4Contrast + _Blur5Contrast + _Blur6Contrast);
					
					float avgColor = pow( tex2Dlod(_AvgTex, float4(UV, 0, 0)).x, 0.45 );

					noBlurTex = tex2Dlod(_BlurTex0, float4(UV, 0, 0)).x - avgColor + 0.5;
					
					half4 blurTex0 = half4((pow(tex2Dlod(_BlurTex0, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur0Contrast + 0.5, 1.0) * _Blur0Weight;
					half4 blurTex1 = half4((pow(tex2Dlod(_BlurTex1, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur1Contrast + 0.5, 1.0) * _Blur1Weight;
					half4 blurTex2 = half4((pow(tex2Dlod(_BlurTex2, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur2Contrast + 0.5, 1.0) * _Blur2Weight;
					half4 blurTex3 = half4((pow(tex2Dlod(_BlurTex3, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur3Contrast + 0.5, 1.0) * _Blur3Weight;
					half4 blurTex4 = half4((pow(tex2Dlod(_BlurTex4, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur4Contrast + 0.5, 1.0) * _Blur4Weight;
					half4 blurTex5 = half4((pow(tex2Dlod(_BlurTex5, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur5Contrast + 0.5, 1.0) * _Blur5Weight;
					half4 blurTex6 = half4((pow(tex2Dlod(_BlurTex6, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur6Contrast + 0.5, 1.0) * _Blur6Weight;

					heightTex = blurTex0 + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;

					heightTex *= 1.0 / heightTex.w;

				}

				if( _Isolate ){
					heightTex.xyz = noBlurTex;
				}else{
					heightTex.xyz = saturate(((heightTex.xxx - 0.5) * _FinalContrast) + 0.5 + _FinalBias);
					
					if( heightTex.x > 0.5 ){
						heightTex.x = pow( saturate( heightTex.x * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5;
					}else{
						heightTex.x = 1.0 - ( pow( saturate( ( 1.0 - heightTex.x ) * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5 );
					}

				}
				
				
				float3 finalColor = lerp( mainTex.xyz, heightTex.xxx, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

				return float4( finalColor, 1 );
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
			#define UNITY_PASS_DEFERRED
			
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			sampler2D _MainTex;
			
			sampler2D _BlurTex0;
			sampler2D _BlurTex1;
			sampler2D _BlurTex2;
			sampler2D _BlurTex3;
			sampler2D _BlurTex4;
			sampler2D _BlurTex5;
			sampler2D _BlurTex6;

			sampler2D _AvgTex;
			
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
			float _FinalBias;
			
			float _GamaCorrection;
			
			float _FinalGain;
			
			float _Slider;

			float _IsNormal;
			
			bool _Isolate;

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
			
				float2 UV = IN.uv;

				float4 heightTex = float4(0,0,0,0);

				half4 mainTex = tex2Dlod(_MainTex, float4(UV, 0, 0));
				half noBlurTex = 0;
				
				if (_IsNormal > 0.5) {
					heightTex = tex2Dlod(_BlurTex0, float4(UV, 0, 0)).xxxx;
				} else {
					float totalContrast = 7.0 / (_Blur0Contrast + _Blur1Contrast + _Blur2Contrast + _Blur3Contrast + _Blur4Contrast + _Blur5Contrast + _Blur6Contrast);
					
					float avgColor = pow( tex2Dlod(_AvgTex, float4(UV, 0, 0)).x, 0.45 );

					noBlurTex = tex2Dlod(_BlurTex0, float4(UV, 0, 0)).x - avgColor + 0.5;
					
					half4 blurTex0 = half4((pow(tex2Dlod(_BlurTex0, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur0Contrast + 0.5, 1.0) * _Blur0Weight;
					half4 blurTex1 = half4((pow(tex2Dlod(_BlurTex1, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur1Contrast + 0.5, 1.0) * _Blur1Weight;
					half4 blurTex2 = half4((pow(tex2Dlod(_BlurTex2, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur2Contrast + 0.5, 1.0) * _Blur2Weight;
					half4 blurTex3 = half4((pow(tex2Dlod(_BlurTex3, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur3Contrast + 0.5, 1.0) * _Blur3Weight;
					half4 blurTex4 = half4((pow(tex2Dlod(_BlurTex4, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur4Contrast + 0.5, 1.0) * _Blur4Weight;
					half4 blurTex5 = half4((pow(tex2Dlod(_BlurTex5, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur5Contrast + 0.5, 1.0) * _Blur5Weight;
					half4 blurTex6 = half4((pow(tex2Dlod(_BlurTex6, float4(UV, 0, 0)).xyz, 0.45) - avgColor) * _Blur6Contrast + 0.5, 1.0) * _Blur6Weight;

					heightTex = blurTex0 + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;

					heightTex *= 1.0 / heightTex.w;

				}

				if( _Isolate ){
					heightTex.xyz = noBlurTex;
				}else{
					heightTex.xyz = saturate(((heightTex.xxx - 0.5) * _FinalContrast) + 0.5 + _FinalBias);
					
					if( heightTex.x > 0.5 ){
						heightTex.x = pow( saturate( heightTex.x * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5;
					}else{
						heightTex.x = 1.0 - ( pow( saturate( ( 1.0 - heightTex.x ) * 2.0 - 1.0 ), _FinalGain ) * 0.5 + 0.5 );
					}

				}
				
				
				float3 finalColor = lerp( mainTex.xyz, heightTex.xxx, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );
				
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
				surfOut.Emission = saturate( pow( finalColor, _GamaCorrection ) );
				//surfOut.Emission = finalColor;
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = 0;
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );

				outNormal.w = 0.33;
				
			}
			ENDCG
		}
		*/
	}
	FallBack "Diffuse"
}
