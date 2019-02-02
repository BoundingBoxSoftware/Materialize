Shader "Custom/Alignment_Shader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CorrectTex ("Base (RGB)", 2D) = "white" {}
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
			sampler2D _CorrectTex;
			
			float2 _PointTL;
			float2 _PointTR;
			float2 _PointBL;
			float2 _PointBR;
			
			float2 _PointScale;
			
			float2 _TargetPoint;
			
			float _Slider;
			
			float _GamaCorrection;

			float minimum_distance(float2 v, float2 w, float2 p) {
				// Return minimum distance between line segment vw and point p
				//const float l2 = length_squared(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
				float l2 = distance(v, w) * distance(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
				if (l2 == 0.0) return distance(p, v);   // v == w case
				// Consider the line extending the segment, parameterized as v + t (w - v).
				// We find projection of point p onto the line. 
				// It falls where t = [(p-v) . (w-v)] / |w-v|^2
				float t = dot(p - v, w - v) / l2;
				if (t < 0.0) return distance(p, v);       // Beyond the 'v' end of the segment
				else if (t > 1.0) return distance(p, w);  // Beyond the 'w' end of the segment
				float2 projection = v + t * (w - v);  // Projection falls on the segment
				return distance(p, projection);
			}

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
				float2 UVPoint = IN.uv * _PointScale;
				half4 c = tex2D (_MainTex, UV);
				half4 c2 = tex2D (_CorrectTex, UV);
				
				float dotSize = 0.025;
				float dotEdge = 0.001;
				
				float2 flippedUV = UV;
				flippedUV.y = 1.0 - flippedUV.y;
				
				float2 pointTL = _PointTL * _PointScale;
				float2 pointTR = _PointTR * _PointScale;
				float2 pointBL = _PointBL * _PointScale;
				float2 pointBR = _PointBR * _PointScale;
				
				float dot1 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointTL, UVPoint) );
				float dot2 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointTR, UVPoint) );
				float dot3 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointBL, UVPoint) );
				float dot4 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointBR, UVPoint) );
				
				float tp = 1.0 - smoothstep( 0.009, 0.010, distance( _TargetPoint * _PointScale, UVPoint) );
				
				float dots = max( max( dot1, dot2 ), max( dot3, dot4 ) );
				
				//float2 newUVtop = lerp( _PointTL, _PointTR, UV.x );
				//float2 newUVbot = lerp( _PointBL, _PointBR, UV.x );
				//float2 newUV = lerp( newUVbot, newUVtop, UV.y );
				//half4 c2 = tex2D (_MainTex, newUV);				
				
				half minDist = minimum_distance(pointTL, pointTR, UVPoint );
				minDist = min( minDist, minimum_distance(pointTR, pointBR, UVPoint ) );
				minDist = min( minDist, minimum_distance(pointBR, pointBL, UVPoint ) );
				minDist = min( minDist, minimum_distance(pointBL, pointTL, UVPoint ) );
				
				float guideLine = 1.0 - smoothstep( 0.004, 0.005, minDist );
				
				c = lerp( c, 1.0, dots * 0.5 );
				c = lerp( c, 1.0, guideLine * 0.3 );
				c = lerp( c, float4(1.0,0.5,0.0,1.0 ), tp );
				
				c2 = lerp( c2, 1.0, dots * 0.2 );
				c2 = lerp( c2, 1.0, guideLine * 0.2 );
				c2 = lerp( c2, float4(1.0,0.5,0.0,1.0 ), tp * 0.5 );
				
				float3 finalColor = lerp( c.xyz, c2.xyz, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

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
			sampler2D _CorrectTex;
			
			float2 _PointTL;
			float2 _PointTR;
			float2 _PointBL;
			float2 _PointBR;
			
			float2 _PointScale;
			
			float2 _TargetPoint;
			
			float _Slider;
			
			float _GamaCorrection;
			
			#include "DNMST.cginc"
			
			float minimum_distance(float2 v, float2 w, float2 p) {
				// Return minimum distance between line segment vw and point p
				//const float l2 = length_squared(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
				float l2 = distance(v, w) * distance(v, w);  // i.e. |w-v|^2 -  avoid a sqrt
				if (l2 == 0.0) return distance(p, v);   // v == w case
				// Consider the line extending the segment, parameterized as v + t (w - v).
				// We find projection of point p onto the line. 
				// It falls where t = [(p-v) . (w-v)] / |w-v|^2
				float t = dot(p - v, w - v) / l2;
				if (t < 0.0) return distance(p, v);       // Beyond the 'v' end of the segment
				else if (t > 1.0) return distance(p, w);  // Beyond the 'w' end of the segment
				float2 projection = v + t * (w - v);  // Projection falls on the segment
				return distance(p, projection);
			}

			void frag_surf (v2f_surf IN, out half4 outDiffuse : SV_Target0, out half4 outMST : SV_Target1, out half4 outNormal : SV_Target2, out half4 outEmission : SV_Target3) {
			
				float2 UV = IN.uv;
				float2 UVPoint = IN.uv * _PointScale;
				half4 c = tex2D (_MainTex, UV);
				half4 c2 = tex2D (_CorrectTex, UV);
				
				float dotSize = 0.025;
				float dotEdge = 0.001;
				
				float2 flippedUV = UV;
				flippedUV.y = 1.0 - flippedUV.y;
				
				float2 pointTL = _PointTL * _PointScale;
				float2 pointTR = _PointTR * _PointScale;
				float2 pointBL = _PointBL * _PointScale;
				float2 pointBR = _PointBR * _PointScale;
				
				float dot1 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointTL, UVPoint) );
				float dot2 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointTR, UVPoint) );
				float dot3 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointBL, UVPoint) );
				float dot4 = 1.0 - smoothstep( dotSize - dotEdge, dotSize, distance(pointBR, UVPoint) );
				
				float tp = 1.0 - smoothstep( 0.009, 0.010, distance( _TargetPoint * _PointScale, UVPoint) );
				
				float dots = max( max( dot1, dot2 ), max( dot3, dot4 ) );
				
				//float2 newUVtop = lerp( _PointTL, _PointTR, UV.x );
				//float2 newUVbot = lerp( _PointBL, _PointBR, UV.x );
				//float2 newUV = lerp( newUVbot, newUVtop, UV.y );
				//half4 c2 = tex2D (_MainTex, newUV);				
				
				half minDist = minimum_distance(pointTL, pointTR, UVPoint );
				minDist = min( minDist, minimum_distance(pointTR, pointBR, UVPoint ) );
				minDist = min( minDist, minimum_distance(pointBR, pointBL, UVPoint ) );
				minDist = min( minDist, minimum_distance(pointBL, pointTL, UVPoint ) );
				
				float guideLine = 1.0 - smoothstep( 0.004, 0.005, minDist );
				
				c = lerp( c, 1.0, dots * 0.5 );
				c = lerp( c, 1.0, guideLine * 0.3 );
				c = lerp( c, float4(1.0,0.5,0.0,1.0 ), tp );
				
				c2 = lerp( c2, 1.0, dots * 0.2 );
				c2 = lerp( c2, 1.0, guideLine * 0.2 );
				c2 = lerp( c2, float4(1.0,0.5,0.0,1.0 ), tp * 0.5 );
				
				float3 finalColor = lerp( c.xyz, c2.xyz, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );
				
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
			}
			ENDCG
		}
		*/
	} 
	FallBack "Diffuse"
}
