// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/PBS_Tess" {
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
		
		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex tessvert_surf
#pragma fragment frag_surf
#pragma hull hs_surf
#pragma domain ds_surf
#pragma target 5.0
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_FOG_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_FOG_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_FOG_COORDS(9)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD10;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_FOG_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex tessvert_surf
#pragma fragment frag_surf
#pragma hull hs_surf
#pragma domain ds_surf
#pragma target 5.0
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd_fullshadows
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  fixed3 tSpace0 : TEXCOORD1;
  fixed3 tSpace1 : TEXCOORD2;
  fixed3 tSpace2 : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  float4 custompack0 : TEXCOORD5; // tSpaceY
  float4 custompack1 : TEXCOORD6; // bSpaceY
  float4 custompack2 : TEXCOORD7; // localNormal
  UNITY_SHADOW_COORDS(8)
  UNITY_FOG_COORDS(9)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos = worldPos;

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex tessvert_surf
#pragma fragment frag_surf
#pragma hull hs_surf
#pragma domain ds_surf
#pragma target 5.0
#pragma multi_compile_instancing
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_DEFERRED
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION

#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_DEFERRED
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION

#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif


ENDCG

}

	// ---- shadow caster pass:
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
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_shadowcaster
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_SHADOWCASTER
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float4 custompack0 : TEXCOORD2; // tSpaceY
  float4 custompack1 : TEXCOORD3; // bSpaceY
  float4 custompack2 : TEXCOORD4; // localNormal
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_SHADOWCASTER
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float4 custompack0 : TEXCOORD2; // tSpaceY
  float4 custompack1 : TEXCOORD3; // bSpaceY
  float4 custompack2 : TEXCOORD4; // localNormal
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif


ENDCG

}

	// ---- meta information extraction pass:
	Pass {
		Name "Meta"
		Tags { "LightMode" = "Meta" }
		Cull Off

CGPROGRAM
// compile directives
#pragma vertex tessvert_surf
#pragma fragment frag_surf
#pragma hull hs_surf
#pragma domain ds_surf
#pragma target 5.0
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma skip_variants INSTANCING_ON
#pragma shader_feature EDITOR_VISUALIZATION

#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: YES
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 1 texcoords actually used
//   float2 _DiffuseMap
#define UNITY_PASS_META
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 31 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard vertex:vert fullforwardshadows addshadow tessellate:tessEdge

		// Use shader model 5.0 target, to get nicer looking lighting
		//#pragma target 5.0

		////#pragma multi_compile _ TOP_PROJECTION
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

		float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength );
		}

		struct Input {
			float2 uv_DiffuseMap;
			float4 tSpaceY;
			float4 bSpaceY;
			float4 localNormal;
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
		// //#pragma instancing_options assumeuniformscaling
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
			o.Normal = texNormal;
			o.Metallic = saturate( _Metallic * texMetallic.x );
			o.Smoothness = saturate( _Smoothness * texSmoothness );
			o.Emission = 0;
			o.Alpha = 1.0;
			o.Occlusion = pow( texAO, max( _AOPower, 0.001 ) );

		}
		

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float4 texcoord : TEXCOORD0;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  float4 texcoord3 : TEXCOORD3;
  half4 color : COLOR;
};
InternalTessInterp_appdata_full tessvert_surf (appdata_full v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.texcoord3 = v.texcoord3;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata_full vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].texcoord3 = v[0].texcoord3;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].texcoord3 = v[1].texcoord3;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].texcoord3 = v[2].texcoord3;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION

#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _DiffuseMap
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // tSpaceY
  float4 custompack1 : TEXCOORD5; // bSpaceY
  float4 custompack2 : TEXCOORD6; // localNormal
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _DiffuseMap_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.tSpaceY;
  o.custompack1.xyzw = customInputData.bSpaceY;
  o.custompack2.xyzw = customInputData.localNormal;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _DiffuseMap);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_full v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_DiffuseMap.x = 1.0;
  surfIN.tSpaceY.x = 1.0;
  surfIN.bSpaceY.x = 1.0;
  surfIN.localNormal.x = 1.0;
  surfIN.uv_DiffuseMap = IN.pack0.xy;
  surfIN.tSpaceY = IN.custompack0.xyzw;
  surfIN.bSpaceY = IN.custompack1.xyzw;
  surfIN.localNormal = IN.custompack2.xyzw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  return UnityMetaFragment(metaIN);
}


#endif


ENDCG

}

	// ---- end of surface shader generated code

#LINE 205

	}
	FallBack "Diffuse"
}
