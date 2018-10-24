Shader "Custom/Edit_Diffuse_Preview" {
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
			float _DiffuseContrast;
			float _DiffuseBias;
			
			sampler2D _BlurTex;
			float _BlurWeight;
			float _BlurContrast;

			sampler2D _AvgTex;
			
			float _LightMaskPow;
			float _LightPow;
			
			float _DarkMaskPow;
			float _DarkPow;

			float _HotSpot;
			float _DarkSpot;

			float _FinalContrast;
			float _FinalBias;
			
			float _ColorLerp;
			
			float _Saturation;
			
			float _GamaCorrection;

			float _Slider;
			
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

				// Texturel lookups
				half3 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xyz;
				half3 blurTex = tex2Dlod(_BlurTex, float4( UV, 0, 0 ) ).xyz;
				half3 avgColor = tex2Dlod(_AvgTex, float4( UV, 0, 0 ) ).xyz;

				half3 mainTexOriginal = mainTex;

				// overlay
				half3 overlay = mainTex - blurTex;

				// save original copy
				half3 maintexInitialHSL = RGBToHSL( mainTex.xyz );

				// spot removal
				//float maxLum = max( max( mainTex.x, mainTex.y ), mainTex.z );
				float avgLum = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
				float lightMask = smoothstep( 1.0 - _HotSpot, ( 1.0 - _HotSpot ) + pow( _HotSpot, 0.5 ) + 0.01, avgLum );
				float darkMask = smoothstep( _DarkSpot - pow( _DarkSpot, 0.5 ) - 0.01, _DarkSpot, avgLum ); // this mask is inverted
				mainTex = lerp( mainTex, avgColor, 1.0 - ( 1.0 - lightMask ) * darkMask );

				// Lighting removal
				float lightMaskPow = saturate( ( _LightMaskPow - 0.5 ) * 2.0 ) + 1.0;
				lightMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _LightMaskPow - 0.5 ) * -2.0 ) + 1.0) );

				float darkMaskPow = saturate( ( _DarkMaskPow - 0.5 ) * 2.0 ) + 1;
				darkMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _DarkMaskPow - 0.5 ) * -2.0 ) + 1.0) );

				mainTex = ( mainTex - avgColor);
				half mainTexGrey = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
				half3 mainTexHighMask = pow( clamp( mainTexGrey * 2.0, 0.001, 0.99 ), lightMaskPow );
				half3 mainTexLowMask = pow( clamp( -mainTexGrey * 2.0, 0.001, 0.99 ), darkMaskPow );
				mainTex += 0.5;

				//mainTex = clamp( mainTex, 0.001, 0.99 );
				//mainTex = lerp( mainTex, pow( mainTex, _LightPow * 5.0 + 1.0 ), mainTexHighMask );
				//mainTex = lerp( mainTex, pow( mainTex, 1.0 / ( _DarkPow * 5.0 + 1.0 ) ), mainTexLowMask );

				mainTex = lerp( mainTex, mainTex * ( 1.0 - _LightPow ) , mainTexHighMask );
				mainTex = lerp( mainTex, 1.0 - ( 1.0 - mainTex ) * ( 1.0 - _DarkPow ), mainTexLowMask );

				float desaturateMask = mainTexHighMask * _LightPow;
				desaturateMask += mainTexLowMask * _DarkPow * 2.0;
				desaturateMask += 1.0 - ( 1.0 - lightMask ) * darkMask;
				desaturateMask = 1.0 - saturate( desaturateMask );

				// apply overlay
				float overlayMask = 1.0 - ( 1.0 - mainTexHighMask * _LightPow ) * ( 1.0 - mainTexLowMask * _DarkPow );
				overlayMask = saturate( overlayMask * 2.0 + 0.1 );
				mainTex *= lerp( 1.0, overlay * _BlurContrast * 10.0 + 1.0, overlayMask );
				mainTex = saturate( mainTex );

				// maintain color
				half3 maintexHSL = RGBToHSL( saturate( mainTex.xyz ) );
				maintexHSL.xy = maintexInitialHSL.xy;
				float3 mainTexOriginalColor = HSLToRGB( maintexHSL );
				mainTex.xyz = lerp( mainTex.xyz, mainTexOriginalColor.xyz, _ColorLerp * desaturateMask );

				// brightness and contrast
				mainTex = saturate( ( mainTex - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );

				// Saturation
				mainTex.xyz = lerp( mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2, mainTex.xyz, _Saturation );

				// Slider
				float3 finalColor = lerp( mainTexOriginal, mainTex, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

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
			float _DiffuseContrast;
			float _DiffuseBias;
			
			sampler2D _BlurTex;
			float _BlurWeight;
			float _BlurContrast;

			sampler2D _AvgTex;
			
			float _LightMaskPow;
			float _LightPow;
			
			float _DarkMaskPow;
			float _DarkPow;

			float _HotSpot;
			float _DarkSpot;

			float _FinalContrast;
			float _FinalBias;
			
			float _ColorLerp;
			
			float _Saturation;
			
			float _GamaCorrection;

			float _Slider;
			
			#include "Photoshop.cginc"

			#include "DNMST.cginc"

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {

				float2 UV = IN.uv;

				// Texturel lookups
				half3 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xyz;
				half3 blurTex = tex2Dlod(_BlurTex, float4( UV, 0, 0 ) ).xyz;
				half3 avgColor = tex2Dlod(_AvgTex, float4( UV, 0, 0 ) ).xyz;

				half3 mainTexOriginal = mainTex;

				// overlay
				half3 overlay = mainTex - blurTex;

				// save original copy
				half3 maintexInitialHSL = RGBToHSL( mainTex.xyz );

				// spot removal
				//float maxLum = max( max( mainTex.x, mainTex.y ), mainTex.z );
				float avgLum = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
				float lightMask = smoothstep( 1.0 - _HotSpot, ( 1.0 - _HotSpot ) + pow( _HotSpot, 0.5 ) + 0.01, avgLum );
				float darkMask = smoothstep( _DarkSpot - pow( _DarkSpot, 0.5 ) - 0.01, _DarkSpot, avgLum ); // this mask is inverted
				mainTex = lerp( mainTex, avgColor, 1.0 - ( 1.0 - lightMask ) * darkMask );

				// Lighting removal
				float lightMaskPow = saturate( ( _LightMaskPow - 0.5 ) * 2.0 ) + 1.0;
				lightMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _LightMaskPow - 0.5 ) * -2.0 ) + 1.0) );

				float darkMaskPow = saturate( ( _DarkMaskPow - 0.5 ) * 2.0 ) + 1;
				darkMaskPow -= 1.0 - ( 1.0 / ( saturate( ( _DarkMaskPow - 0.5 ) * -2.0 ) + 1.0) );

				mainTex = ( mainTex - avgColor);
				half mainTexGrey = mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2;
				half3 mainTexHighMask = pow( clamp( mainTexGrey * 2.0, 0.001, 0.99 ), lightMaskPow );
				half3 mainTexLowMask = pow( clamp( -mainTexGrey * 2.0, 0.001, 0.99 ), darkMaskPow );
				mainTex += 0.5;

				//mainTex = clamp( mainTex, 0.001, 0.99 );
				//mainTex = lerp( mainTex, pow( mainTex, _LightPow * 5.0 + 1.0 ), mainTexHighMask );
				//mainTex = lerp( mainTex, pow( mainTex, 1.0 / ( _DarkPow * 5.0 + 1.0 ) ), mainTexLowMask );

				mainTex = lerp( mainTex, mainTex * ( 1.0 - _LightPow ) , mainTexHighMask );
				mainTex = lerp( mainTex, 1.0 - ( 1.0 - mainTex ) * ( 1.0 - _DarkPow ), mainTexLowMask );

				float desaturateMask = mainTexHighMask * _LightPow;
				desaturateMask += mainTexLowMask * _DarkPow * 2.0;
				desaturateMask += 1.0 - ( 1.0 - lightMask ) * darkMask;
				desaturateMask = 1.0 - saturate( desaturateMask );

				// apply overlay
				float overlayMask = 1.0 - ( 1.0 - mainTexHighMask * _LightPow ) * ( 1.0 - mainTexLowMask * _DarkPow );
				overlayMask = saturate( overlayMask * 2.0 + 0.1 );
				mainTex *= lerp( 1.0, overlay * _BlurContrast * 10.0 + 1.0, overlayMask );
				mainTex = saturate( mainTex );

				// maintain color
				half3 maintexHSL = RGBToHSL( saturate( mainTex.xyz ) );
				maintexHSL.xy = maintexInitialHSL.xy;
				float3 mainTexOriginalColor = HSLToRGB( maintexHSL );
				mainTex.xyz = lerp( mainTex.xyz, mainTexOriginalColor.xyz, _ColorLerp * desaturateMask );

				// brightness and contrast
				mainTex = saturate( ( mainTex - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );

				// Saturation
				mainTex.xyz = lerp( mainTex.x * 0.3 + mainTex.y * 0.5 + mainTex.z * 0.2, mainTex.xyz, _Saturation );

				// Slider
				float3 finalColor = lerp( mainTexOriginal, mainTex, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

				//finalColor = desaturateMask;

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
