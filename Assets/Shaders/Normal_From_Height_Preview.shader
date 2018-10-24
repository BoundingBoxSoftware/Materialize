Shader "Custom/Normal_From_Height_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex0 ("Base (RGB)", 2D) = "white" {}
		_BlurTex1 ("Base (RGB)", 2D) = "white" {}
		_BlurTex2 ("Base (RGB)", 2D) = "white" {}
		_BlurTex3 ("Base (RGB)", 2D) = "white" {}
		_BlurTex4 ("Base (RGB)", 2D) = "white" {}
		_BlurTex5 ("Base (RGB)", 2D) = "white" {}
		_BlurTex6 ("Base (RGB)", 2D) = "white" {}
		
		//_SlopeBias ("Slope Bias", Float) = 0.0
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
			
			sampler2D _HeightTex;
			sampler2D _HeightBlurTex;
			
			sampler2D _BlurTex0;
			sampler2D _BlurTex1;
			sampler2D _BlurTex2;
			sampler2D _BlurTex3;
			sampler2D _BlurTex4;
			sampler2D _BlurTex5;
			sampler2D _BlurTex6;
			
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
			
			float _GamaCorrection;
			
			float _SlopeBias;
			float _ShapeRecognition;
			float _LightRotation;
			
			float _Angularity;
			float _AngularIntensity;
			
			int _FlipNormalY;

			float _Slider;

			float3 _LightDir;

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

				half4 mainTex = half4( tex2Dlod(_BlurTex0, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur0Weight;
				half4 blurTex1 = half4( tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur1Weight;
				half4 blurTex2 = half4( tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur2Weight;
				half4 blurTex3 = half4( tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur3Weight;
				half4 blurTex4 = half4( tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur4Weight;
				half4 blurTex5 = half4( tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur5Weight;
				half4 blurTex6 = half4( tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur6Weight;
				
				mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
				
				mainTex *= 1.0 / mainTex.w;
				
				mainTex.xyz = normalize( mainTex.xyz * 2.0 - 1.0 );
				
				float3 angularDir = normalize( float3( normalize( float3( mainTex.xy, 0.01 ) ).xy * _AngularIntensity, max( 1.0 - _AngularIntensity, 0.01 ) ) );
				mainTex.xyz = lerp( mainTex.xyz, angularDir, _Angularity );
				
				mainTex.xy = mainTex.xy * max( _FinalContrast, 0.01 );
				mainTex.z = pow( saturate( mainTex.z ), max( _FinalContrast, 0.01 ) );
				
				mainTex.xyz = normalize( mainTex.xyz ) * 0.5 + 0.5;

				return float4( mainTex.xyz, 1 );
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
			
			sampler2D _HeightTex;
			sampler2D _HeightBlurTex;
			
			sampler2D _BlurTex0;
			sampler2D _BlurTex1;
			sampler2D _BlurTex2;
			sampler2D _BlurTex3;
			sampler2D _BlurTex4;
			sampler2D _BlurTex5;
			sampler2D _BlurTex6;
			
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
			
			float _GamaCorrection;
			
			float _SlopeBias;
			float _ShapeRecognition;
			float _LightRotation;
			
			float _Angularity;
			float _AngularIntensity;
			
			int _FlipNormalY;

			float _Slider;

			float3 _LightDir;

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
			
				float2 UV = IN.uv;


				//half4 testTex = tex2Dlod(_BlurTex0, float4( UV, 0, 0 ) );

				half4 mainTex = half4( tex2Dlod(_BlurTex0, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur0Weight;
				half4 blurTex1 = half4( tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur1Weight;
				half4 blurTex2 = half4( tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur2Weight;
				half4 blurTex3 = half4( tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur3Weight;
				half4 blurTex4 = half4( tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur4Weight;
				half4 blurTex5 = half4( tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur5Weight;
				half4 blurTex6 = half4( tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur6Weight;
				
				mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
				
				mainTex *= 1.0 / mainTex.w;
				
				mainTex.xyz = normalize( mainTex.xyz * 2.0 - 1.0 );
				
				float3 angularDir = normalize( float3( normalize( float3( mainTex.xy, 0.01 ) ).xy * _AngularIntensity, max( 1.0 - _AngularIntensity, 0.01 ) ) );
				mainTex.xyz = lerp( mainTex.xyz, angularDir, _Angularity );
				
				mainTex.xy = mainTex.xy * max( _FinalContrast, 0.01 );
				mainTex.z = pow( saturate( mainTex.z ), max( _FinalContrast, 0.01 ) );
				
				mainTex.xyz = normalize( mainTex.xyz );
				
				if( _FlipNormalY == 0 ){
					//mainTex.y *= -1.0;
				}
								
				//Deferred Stuff
				
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );	
				
				fixed3 localNormal = mainTex.xyz;
				
				fixed3 worldNormal;
				worldNormal.x = dot(IN.tSpace0.xyz, localNormal);
				worldNormal.y = dot(IN.tSpace1.xyz, localNormal);
				worldNormal.z = dot(IN.tSpace2.xyz, localNormal);
				worldNormal = normalize( worldNormal );

				float reveal = smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x );

				float lightDot = -dot( worldNormal, _LightDir );
				lightDot = lightDot * 0.5 + 0.5;
				lightDot *= lightDot;

				if( _FlipNormalY == 0 ){
					localNormal.y *= -1.0;
				}
				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = 0.0;//lerp( pow( float3(0.7,0.7,0.7), _GamaCorrection ),0.0, reveal );
				surfOut.Normal = worldNormal.xyz;
				surfOut.Metallic = 0;
				surfOut.Smoothness = 0;
				surfOut.Transmission = 0;
				surfOut.Emission = pow( lerp( lightDot.xxx /*float3(0,0,0)*/, saturate( localNormal.xyz * 0.5 + 0.5 ), reveal ), _GamaCorrection );
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = 0.0;
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );
				
			}
			ENDCG
		}
		*/
	} 
	FallBack "Diffuse"
}
