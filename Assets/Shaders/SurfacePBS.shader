Shader "Custom/SurfacePBS" {
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

		//_TopProj ("Top Projection", Float ) = 0.0

		_DispOffset ("Displacement Offset", Float ) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow noforwardadd nometa nolightmap nodynlightmap nodirlightmap exclude_path:deferred exclude_path:prepass //tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		#pragma target 5.0

		#pragma multi_compile _ TOP_PROJECTION
		#include "UnityCG.cginc"
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

		//float _TopProj;

		float _Parallax;
		float _DispOffset;
		float _EdgeLength;

		samplerCUBE _ProbeCubemap;

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
			float3 worldNormal;
			INTERNAL_DATA
		};

		void vert (inout appdata_full v, out Input o){
		//void vert (inout appdata_full v){
			UNITY_INITIALIZE_OUTPUT(Input,o);
			fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
			fixed3 worldTangentY = float3(1,0,0);
			if( abs( v.normal.z ) != 1 ){
				worldTangentY = normalize( cross(v.normal.xyz, float3(0,0,-1) ) );
			}
			worldTangentY = normalize( mul( unity_ObjectToWorld, float4( worldTangentY, 0 ) ).xyz );
			o.tSpaceY = float4( worldTangentY, v.vertex.x );
			o.bSpaceY = float4( normalize( cross( worldNormal, worldTangentY ) ), v.vertex.y );
			o.localNormal = float4( v.normal.xyz, v.vertex.z );

			float2 UV = v.texcoord.xy * _Tiling.xy + _Tiling.zw;
			float d = ( tex2Dlod(_DisplacementMap, float4(UV,0,0)).x - _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

			#ifdef TOP_PROJECTION
				float2 TPUV = v.vertex.xz * 0.14 * _Tiling.xy + _Tiling.zw + 0.5;
				float dy = ( tex2Dlod(_DisplacementMap, float4(TPUV,0,0)).x - _DispOffset ) * _Parallax * ( 1.0 / _Tiling.x );

				float TexBlend = 1.0 - smoothstep( 0.25, 0.75, abs( v.normal.y * 0.8 ) );
				d = lerp( dy, d, TexBlend );
			#endif

			v.vertex.xyz += v.normal * d;

		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float2 UV = IN.uv_DiffuseMap.xy * _Tiling.xy + _Tiling.zw;
			
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

			fixed3 worldNormal = WorldNormalVector (IN, texNormal );

			// Get the tangent and binormal for the texcoord0 (this is just the actual tangent and binormal that comes in from the vertex shader)
			float3 worldVertTangent = WorldNormalVector (IN, float3(1,0,0) );
			float3 worldVertBinormal = WorldNormalVector (IN, float3(0,1,0) );
			float3 worldVertNormal = WorldNormalVector (IN, float3(0,0,1) );

			#ifdef TOP_PROJECTION

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

				worldNormalY = ( worldNormalY.x * IN.tSpaceY.xyz ) + ( worldNormalY.y * IN.bSpaceY.xyz ) + (worldNormalY.z * worldVertNormal );

				half blendY = texDisplaceY + ( abs( IN.localNormal.y * 3.0 ) - 1.5 );

				half blendFalloff = 0.2;
				half texBlend = smoothstep( -blendFalloff, blendFalloff, texDisplace - blendY );
				//half texBlend = 1.0 - smoothstep( 0.25, 0.75, abs( IN.localNormal.y ) );

				texDiffuse = lerp( texDiffuseY, texDiffuse, texBlend );
				worldNormal = normalize( lerp( worldNormalY, worldNormal, texBlend ) );
				texMetallic = lerp( texMetallicY, texMetallic, texBlend );
				texSmoothness = lerp( texSmoothnessY, texSmoothness, texBlend );
				texAO = lerp( texAOY, texAO, texBlend );
				texEdge = lerp( texEdgeY, texEdge, texBlend );

			#endif

			// Convert the world normal to tangent normal
			float3 tangentNormal = 0;
			tangentNormal.x = dot( worldVertTangent, worldNormal );
			tangentNormal.y = dot( worldVertBinormal, worldNormal );
			tangentNormal.z = dot( worldVertNormal, worldNormal );

			texDiffuse.xyz *= ( texEdge - 0.5 ) * _EdgePower + 1.0;
			texDiffuse.xyz = saturate( texDiffuse.xyz );
			texAO = saturate( texAO * ( texEdge + 0.5 ) );


			o.Albedo = texDiffuse.xyz;//saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
			o.Normal = tangentNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );
			half3 ambIBL = texCUBElod(_ProbeCubemap, half4( normalize( worldNormal ), 7 ) ).xyz;
			o.Emission = texDiffuse.xyz * ambIBL * o.Occlusion * 0.5;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
