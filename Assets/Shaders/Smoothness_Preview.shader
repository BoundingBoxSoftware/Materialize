Shader "Custom/Smoothness_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex ("Base (RGB)", 2D) = "white" {}
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
			sampler2D _BlurTex;
			sampler2D _OverlayBlurTex;
			sampler2D _MetallicTex;
			
			float4 _SampleColor1;
			float2 _SampleUV1;

			float4 _SampleColor2;
			float2 _SampleUV2;

			float4 _SampleColor3;
			float2 _SampleUV3;

			float _MetalSmoothness;
			
			int _UseSample1;
			int _IsolateSample1;
			float _HueWeight1;
			float _SatWeight1;
			float _LumWeight1;
			float _MaskLow1;
			float _MaskHigh1;
			float _Sample1Smoothness;

			int _UseSample2;
			int _IsolateSample2;
			float _HueWeight2;
			float _SatWeight2;
			float _LumWeight2;
			float _MaskLow2;
			float _MaskHigh2;
			float _Sample2Smoothness;

			int _UseSample3;
			int _IsolateSample3;
			float _HueWeight3;
			float _SatWeight3;
			float _LumWeight3;
			float _MaskLow3;
			float _MaskHigh3;
			float _Sample3Smoothness;

			float _BaseSmoothness;
			
			float _BlurOverlay;
	
			float _FinalContrast;
	
			float _FinalBias;
			
			float _Slider;
			
			float _GamaCorrection;
			
			int _Invert;
			
			#include "Photoshop.cginc"

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

				half3 mainTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) ).xyz;
				half3 blurTex = tex2Dlod( _BlurTex, float4( UV, 0, 0 ) ).xyz;
				half3 overlayBlurTex = tex2Dlod( _OverlayBlurTex, float4( UV, 0, 0 ) ).xyz;
				
				half3 overlay = ( mainTex - overlayBlurTex );
				half overlayGrey = overlay.x * 0.3 + overlay.y * 0.5 + overlay.z * 0.2;
				
				half metalMask = tex2Dlod( _MetallicTex, float4( UV, 0, 0 ) ).x;
				
				half3 mainTexHSL = RGBToHSL( mainTex.xyz );
				half3 blurTexHSL = RGBToHSL( blurTex.xyz );
				
				float sample1Mask = 0.0;
				if( _UseSample1 ){
					half3 sample1HSL = RGBToHSL( _SampleColor1.xyz );
					float sample1HueDif = 1.0 - min( min( abs( blurTexHSL.x - sample1HSL.x ), abs( ( blurTexHSL.x + 1.0 ) - sample1HSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - sample1HSL.x ) ) * 2.0;
					float sample1SatDif = 1.0 - abs( blurTexHSL.y - sample1HSL.y );
					float sample1LumDif = 1.0 - abs( blurTexHSL.z - sample1HSL.z );
					sample1Mask = ( sample1HueDif * _HueWeight1 ) + ( sample1SatDif * _SatWeight1 ) + ( sample1LumDif * _LumWeight1 );
					sample1Mask *= 1.0 / ( _HueWeight1 + _SatWeight1 + _LumWeight1 );
					sample1Mask = smoothstep( _MaskLow1, _MaskHigh1, sample1Mask );
				}
				
				float sample2Mask = 0.0;
				if( _UseSample2 ){
					half3 sample2HSL = RGBToHSL( _SampleColor2.xyz );
					float sample2HueDif = 1.0 - min( min( abs( blurTexHSL.x - sample2HSL.x ), abs( ( blurTexHSL.x + 1.0 ) - sample2HSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - sample2HSL.x ) ) * 2.0;
					float sample2SatDif = 1.0 - abs( blurTexHSL.y - sample2HSL.y );
					float sample2LumDif = 1.0 - abs( blurTexHSL.z - sample2HSL.z );
					sample2Mask = ( sample2HueDif * _HueWeight2 ) + ( sample2SatDif * _SatWeight2 ) + ( sample2LumDif * _LumWeight2 );
					sample2Mask *= 1.0 / ( _HueWeight2 + _SatWeight2 + _LumWeight2 );
					sample2Mask = smoothstep( _MaskLow2, _MaskHigh2, sample2Mask );
				}

				
				float finalSmoothness = _BaseSmoothness;
				finalSmoothness = lerp( finalSmoothness, _Sample2Smoothness, sample2Mask );
				finalSmoothness = lerp( finalSmoothness, _Sample1Smoothness, sample1Mask );
				finalSmoothness = lerp( finalSmoothness, _MetalSmoothness, metalMask );
				
				finalSmoothness *= clamp( overlayGrey * _BlurOverlay + 1.0, 0.0, 10.0 );

				if (_Invert) {
					finalSmoothness = saturate((finalSmoothness - 0.5) * ((_FinalContrast + 0.5) + _FinalBias) *-1);
				}
				else {
					finalSmoothness = saturate((finalSmoothness - 0.5) * _FinalContrast + 0.5 + _FinalBias);
				}
				finalSmoothness = saturate( finalSmoothness  );
				
				if( _IsolateSample1 == 1 ){
					finalSmoothness = sample1Mask;
				}
				
				if( _IsolateSample2 == 1 ){
					finalSmoothness = sample2Mask;
				}
				float3 finalColor;
				
				finalColor = lerp(mainTex.xyz, finalSmoothness.xxx, smoothstep(_Slider - 0.01, _Slider + 0.01, UV.x));
								

				return float4( finalColor , 1 );
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
			sampler2D _BlurTex;
			sampler2D _OverlayBlurTex;
			sampler2D _MetallicTex;
			
			float4 _SampleColor1;
			float2 _SampleUV1;

			float4 _SampleColor2;
			float2 _SampleUV2;

			float4 _SampleColor3;
			float2 _SampleUV3;

			float _MetalSmoothness;
			
			int _UseSample1;
			int _IsolateSample1;
			float _HueWeight1;
			float _SatWeight1;
			float _LumWeight1;
			float _MaskLow1;
			float _MaskHigh1;
			float _Sample1Smoothness;

			int _UseSample2;
			int _IsolateSample2;
			float _HueWeight2;
			float _SatWeight2;
			float _LumWeight2;
			float _MaskLow2;
			float _MaskHigh2;
			float _Sample2Smoothness;

			int _UseSample3;
			int _IsolateSample3;
			float _HueWeight3;
			float _SatWeight3;
			float _LumWeight3;
			float _MaskLow3;
			float _MaskHigh3;
			float _Sample3Smoothness;

			float _BaseSmoothness;
			
			float _BlurOverlay;
	
			float _FinalContrast;
	
			float _FinalBias;
			
			float _Slider;
			
			float _GamaCorrection;
			
			#include "Photoshop.cginc"

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {

				float2 UV = IN.uv;

				half3 mainTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) ).xyz;
				half3 blurTex = tex2Dlod( _BlurTex, float4( UV, 0, 0 ) ).xyz;
				half3 overlayBlurTex = tex2Dlod( _OverlayBlurTex, float4( UV, 0, 0 ) ).xyz;
				
				half3 overlay = ( mainTex - overlayBlurTex );
				half overlayGrey = overlay.x * 0.3 + overlay.y * 0.5 + overlay.z * 0.2;
				
				half metalMask = tex2Dlod( _MetallicTex, float4( UV, 0, 0 ) ).x;
				
				half3 mainTexHSL = RGBToHSL( mainTex.xyz );
				half3 blurTexHSL = RGBToHSL( blurTex.xyz );
				
				float sample1Mask = 0.0;
				if( _UseSample1 ){
					half3 sample1HSL = RGBToHSL( _SampleColor1.xyz );
					float sample1HueDif = 1.0 - min( min( abs( blurTexHSL.x - sample1HSL.x ), abs( ( blurTexHSL.x + 1.0 ) - sample1HSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - sample1HSL.x ) ) * 2.0;
					float sample1SatDif = 1.0 - abs( blurTexHSL.y - sample1HSL.y );
					float sample1LumDif = 1.0 - abs( blurTexHSL.z - sample1HSL.z );
					sample1Mask = ( sample1HueDif * _HueWeight1 ) + ( sample1SatDif * _SatWeight1 ) + ( sample1LumDif * _LumWeight1 );
					sample1Mask *= 1.0 / ( _HueWeight1 + _SatWeight1 + _LumWeight1 );
					sample1Mask = smoothstep( _MaskLow1, _MaskHigh1, sample1Mask );
				}
				
				float sample2Mask = 0.0;
				if( _UseSample2 ){
					half3 sample2HSL = RGBToHSL( _SampleColor2.xyz );
					float sample2HueDif = 1.0 - min( min( abs( blurTexHSL.x - sample2HSL.x ), abs( ( blurTexHSL.x + 1.0 ) - sample2HSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - sample2HSL.x ) ) * 2.0;
					float sample2SatDif = 1.0 - abs( blurTexHSL.y - sample2HSL.y );
					float sample2LumDif = 1.0 - abs( blurTexHSL.z - sample2HSL.z );
					sample2Mask = ( sample2HueDif * _HueWeight2 ) + ( sample2SatDif * _SatWeight2 ) + ( sample2LumDif * _LumWeight2 );
					sample2Mask *= 1.0 / ( _HueWeight2 + _SatWeight2 + _LumWeight2 );
					sample2Mask = smoothstep( _MaskLow2, _MaskHigh2, sample2Mask );
				}
				
				float finalSmoothness = _BaseSmoothness;
				finalSmoothness = lerp( finalSmoothness, _Sample2Smoothness, sample2Mask );
				finalSmoothness = lerp( finalSmoothness, _Sample1Smoothness, sample1Mask );
				finalSmoothness = lerp( finalSmoothness, _MetalSmoothness, metalMask );
				
				finalSmoothness *= clamp( overlayGrey * _BlurOverlay + 1.0, 0.0, 10.0 );

				finalSmoothness = saturate( ( finalSmoothness - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );

				finalSmoothness = saturate( finalSmoothness );
				
				if( _IsolateSample1 == 1 ){
					finalSmoothness = sample1Mask;
				}
				
				if( _IsolateSample2 == 1 ){
					finalSmoothness = sample2Mask;
				}
				
				float3 finalColor = lerp( mainTex.xyz, finalSmoothness.xxx, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

				
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
				surfOut.Emission = saturate( pow( finalColor.xyz, _GamaCorrection ) );
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
