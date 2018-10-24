// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DNMST_Displace" {
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
		//_GamaCorrection ("Gamma Correction", Float ) = 1.0
		
		_Tiling ("Tiling", Vector ) = (1.0,1.0,0.0,0.0)

		_TopProj ("Top Projection", Float ) = 0.0

		_DispOffset ("Displacement Offset", Float ) = 1.0
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
			// compile directives
			#pragma vertex tessvert_surf
			#pragma fragment frag_surf
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma target 5.0
			#pragma exclude_renderers nomrt
			#pragma multi_compile_prepassfinal

			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
	
			#define UNITY_PASS_DEFERRED
			#define TRIPLANAR
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
			
			// Original surface shader snippet:
			#line 20 ""
			#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
			#endif
			
			#include "Tessellation.cginc"
			
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

			float _TopProj;

			float _DispOffset;
					
			#include "DNMST.cginc"
			
			#include "Tess.cginc"
			
			// vertex shader
			v2f_surf vert_displace (appdata v) {
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				
				float2 UV = v.texcoord.xy * _Tiling.xy + _Tiling.zw;
				float d = ( tex2Dlod(_DisplacementMap, float4(UV,0,0)).x - _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

				float2 TPUV = v.vertex.xz * 0.14 * _Tiling.xy + _Tiling.zw + 0.5;
				float dy = ( tex2Dlod(_DisplacementMap, float4(TPUV,0,0)).x - _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

				float TexBlend = 1.0 - smoothstep( 0.25, 0.75, abs( v.normal.y * 0.8 ) );
				d = lerp( dy, d, TexBlend );

				float3 localPos = v.vertex.xyz;

				v.vertex.xyz += v.normal * d;
				
				float4 lastVertex = v.vertex;
			
				o.pos = UnityObjectToClipPos ( v.vertex );
				 
				o.uv.xy = v.texcoord.xy;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
				o.color = v.color;

				// Y projection
				fixed3 worldTangentY = float3(1,0,0);
				if( abs( v.normal.z ) != 1 ){
					worldTangentY = cross(v.normal.xyz, float3(0,0,-1) );
					worldTangentY = normalize( mul( unity_ObjectToWorld, float4( worldTangentY, 0 ) ).xyz );
				}
				o.tSpaceY = float4( worldTangentY, localPos.x );
				o.bSpaceY = float4( normalize( cross( worldNormal, worldTangentY ) ), localPos.y );
				o.localNormal = float4( v.normal.xyz, localPos.z );
			  	
			  	// Motion Blur
			  	//float4 screenPos = ComputeScreenPos( o.pos );
			  	
			  	float4 screenPos = ComputeScreenPos( mul ( _CURRENT_VP_MATRIX, float4( worldPos, 1 ) ) );		
				float4 lastScreenPos;
				if( _IsMover == 1 ){
					float3 lastWorldPosition = mul ( _LAST_MODEL_MATRIX, v.vertex ).xyz;
					lastScreenPos = ComputeScreenPos( mul ( _LAST_VP_MATRIX, float4( lastWorldPosition, 1 ) ) );
				}else{
					lastScreenPos = ComputeScreenPos( mul ( _LAST_VP_MATRIX, float4( worldPos, 1 ) ) );	
				}
				o.screenPos = screenPos;
				o.lastScreenPos = lastScreenPos;
				
				// Enlighten Global Illumination
				#ifndef DYNAMICLIGHTMAP_OFF
				o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#else
				  o.lmap.zw = 0;
				#endif
				#ifndef LIGHTMAP_OFF
				  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				  #ifdef DIRLIGHTMAP_OFF
				    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
				    o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
				  #endif
				#else
				  o.lmap.xy = 0;
				  #if UNITY_SHOULD_SAMPLE_SH
				    #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
				      o.sh = 0;
				    #elif (SHADER_TARGET < 30)
				      o.sh = ShadeSH9 (float4(worldNormal,1.0));
				    #else
				      o.sh = ShadeSH3Order (half4(worldNormal, 1.0));
				    #endif
				  #endif
				#endif
				
				return o;
			}
			
#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata,3> vi, float3 bary : SV_DomainLocation) {
  appdata v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_displace (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION
			

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
				// Albedo comes from a texture tinted by color
				
				float2 UV = IN.uv.xy * _Tiling.xy + _Tiling.zw;
			
				half3 texDiffuse = tex2D (_DiffuseMap, UV).xyz;
				half3 texNormal = tex2D(_NormalMap,UV).xyz;
				half3 texMetallic = tex2D(_MetallicMap,UV).xyz;
				half texSmoothness = tex2D(_SmoothnessMap,UV).x;
				half texAO = tex2D(_AOMap,UV).x;
				half texEdge = tex2D(_EdgeMap,UV).x;
				half texDisplace = tex2D(_DisplacementMap, UV).x;
				
				texNormal.xyz = texNormal.xyz * 2.0 - 1.0;
				if( _FlipNormalY == 0 ){
					texNormal.y *= -1.0;
				}
				
				fixed3 worldNormal;
				worldNormal.x = dot(IN.tSpace0.xyz, texNormal.xyz);
				worldNormal.y = dot(IN.tSpace1.xyz, texNormal.xyz);
				worldNormal.z = dot(IN.tSpace2.xyz, texNormal.xyz);
				worldNormal = normalize( worldNormal );

				//if( _TopProj == 1 ){

					float3 localPos = float3( IN.tSpaceY.w, IN.bSpaceY.w, IN.localNormal.w );

					float2 TPUV = localPos.xz * 0.14 * _Tiling.xy + _Tiling.zw + 0.5;

					half3 texDiffuseY = tex2D (_DiffuseMap, TPUV).xyz;
					half3 texNormalY = tex2D(_NormalMap,TPUV).xyz;
					half3 texMetallicY = tex2D(_MetallicMap,TPUV).xyz;
					half texSmoothnessY = tex2D(_SmoothnessMap,TPUV).x;
					half texAOY = tex2D(_AOMap,TPUV).x;
					half texEdgeY = tex2D(_EdgeMap,TPUV).x;
					half texDisplaceY = tex2D(_DisplacementMap, TPUV).x;

					texNormalY.xyz = texNormalY.xyz * 2.0 - 1.0;
					if( _FlipNormalY == 0 ){
						texNormalY.y *= -1.0;
					}

					texNormalY.x *= step( IN.localNormal.y, 0 ) * 2 - 1;

					float3 worldNormalY = texNormalY.xyz;

					float3 worldVertNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );

					worldNormalY = ( worldNormalY.x * IN.tSpaceY.xyz ) + ( worldNormalY.y * IN.bSpaceY.xyz ) + (worldNormalY.z * worldVertNormal );

					half blendY = texDisplaceY + ( abs( IN.localNormal.y * 3.0 ) - 1.5 );

					half blendFalloff = 0.1;
					half SSHigh = 0.01 + ( saturate( blendFalloff ) );
					half SSLow = -0.01 - ( saturate( blendFalloff ) );

					half texBlend = smoothstep( SSLow, SSHigh, texDisplace - blendY );
					//half texBlend = 1.0 - smoothstep( 0.25, 0.75, abs( IN.localNormal.y ) );

					texDiffuse = lerp( texDiffuseY, texDiffuse, texBlend );
					worldNormal = normalize( lerp( worldNormalY, worldNormal, texBlend ) );
					texMetallic = lerp( texMetallicY, texMetallic, texBlend );
					texSmoothness = lerp( texSmoothnessY, texSmoothness, texBlend );
					texAO = lerp( texAOY, texAO, texBlend );
					texEdge = lerp( texEdgeY, texEdge, texBlend );

				//}


				// Deferred stuff
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );

				
				//float fresnel = saturate( dot( worldViewDir, worldNormal ) * 0.5 + 0.5 );
				//float schlickFresnel = pow( 1.0 - fresnel, 3.0 );

				//texDiffuse.xyz = pow( texDiffuse.xyz, 1.0 - saturate( saturate( _Dust ) * fresnel ) );
				texDiffuse.xyz *= ( texEdge - 0.5 ) * _EdgePower + 1.0;
				//texDiffuse.xyz = pow( texDiffuse.xyz, ( 1.0 - texAO ) * _AOPower + 1.0 );
				
				texAO = saturate( texAO * ( texEdge + 0.5 ) );

				texDiffuse.xyz = saturate( texDiffuse.xyz );
				
				SurfaceOutputStandard surfOut;
				surfOut.Albedo = saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
				surfOut.Normal = worldNormal.xyz;
				surfOut.Metallic = saturate( _Metallic * texMetallic.x );
				//surfOut.Smoothness = saturate( lerp( saturate( _Smoothness * texSmoothness ), 1.0, schlickFresnel ) );
				//surfOut.Smoothness = saturate( _Smoothness * pow( texSmoothness, fresnel + 0.5 ) );
				surfOut.Smoothness = saturate( _Smoothness * texSmoothness );
				surfOut.Transmission = 0.0;
				surfOut.Emission = 0;
				surfOut.Motion = CalcMotionVector( IN );
				surfOut.Alpha = 1.0;
				surfOut.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );
				
				//surfOut.Albedo = 0;
				//surfOut.Occlusion = 0;
				//surfOut.Smoothness = 0;
				//surfOut.Emission = saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
				
				ReturnOutput ( surfOut, worldPos, worldViewDir, IN, outDiffuse, outMST, outNormal, outEmission );
				
				
			}
				
			ENDCG
		}
		
		
//======================================================//
//					Shadow Castor Pass					//
//======================================================//


		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual

			CGPROGRAM
			// compile directives
			#pragma vertex tessvert_surf
			#pragma fragment frag_surf
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			
			#define UNITY_PASS_SHADOWCASTER
			#define TRIPLANAR
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			
			#define INTERNAL_DATA
			#define WorldReflectionVector(data,normal) data.worldRefl
			#define WorldNormalVector(data,normal) normal
			
			// Original surface shader snippet:
			#line 20 ""
			#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
			#endif

			#include "Tessellation.cginc"
	
			sampler2D _DisplacementMap;
			float _DispOffset;
			float4 _Tiling;
			
			//#include "DNMST.cginc"
	
			#include "Tess.cginc"

			// vertex-to-fragment interpolation data
			struct v2f_shadow {
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
			};
			
			// vertex shader
			v2f_shadow vert_shadow (appdata v) {
				v2f_shadow o;
				UNITY_INITIALIZE_OUTPUT(v2f_shadow,o);
			  
				float2 UV = v.texcoord.xy * _Tiling.xy + _Tiling.zw;
				float d = ( tex2Dlod(_DisplacementMap, float4(UV,0,0)).x -  _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

				float2 TPUV = v.vertex.xz * 0.14 * _Tiling.xy + _Tiling.zw + 0.5;
				float dy = ( tex2Dlod(_DisplacementMap, float4(TPUV,0,0)).x - _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

				float TexBlend = 1.0 - smoothstep( 0.2, 0.8, abs( v.normal.y * 0.8  ) );
				d = lerp( dy, d, TexBlend );

				v.vertex.xyz += v.normal * d;
						
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_shadow ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata,3> vi, float3 bary : SV_DomainLocation) {
  appdata v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_shadow o = vert_shadow (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


			// fragment shader
			fixed4 frag_surf (v2f_shadow IN) : SV_Target {
				// prepare and unpack data				
				SHADOW_CASTER_FRAGMENT(IN)
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
	FallBack "Custom/DNMST"
}
