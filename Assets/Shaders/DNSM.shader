Shader "Custom/DNSM" {
	Properties {
		_Diffuse("Diffuse", 2D) = "grey" {}
		_Normal("Normal", 2D) = "bump" {}
		_Smoothness ("Smoothness", 2D) = "grey" {}
		_Metallic ("Metallic", 2D) = "black" {}
		_AO("Ambient Occlusion", 2D) = "white" {}
		_Edge("Edge", 2D) = "grey" {}
		_Displacement("Displacement", 2D) = "grey" {}
		
		_Tint("Diffuse Tint", Color) = (0.5,0.5,0.5,1)
		
		_AOPower ("AO Power", Float ) = 1.0
		_EdgePower ("Edge Power", Float ) = 1.0

		_Shininess ("Shininess", Float ) = 1.0
		_Dust ("Dust", Float ) = 0.0
		
		_Parallax ("Height", Range (0.0, 3.0)) = 0.5
		_EdgeLength ("Edge length", Range(3,50)) = 3

		_Tiling("Tiling", Vector) = (1.0,1.0,0.0,0.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows addshadow vertex:disp tessellate:tessEdge
		#include "Tessellation.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _Diffuse;
		sampler2D _Normal;
		sampler2D _Smoothness;
		sampler2D _Metallic;
		sampler2D _AO;
		sampler2D _Edge;
		sampler2D _Displacement;
		
		float4 _Tiling;
		
		float4 _Tint;
		float _AOPower;
		float _EdgePower;
		float _Shininess;
		float _Dust;
		
		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
		};
		
		struct Input {
			float2 uv_Diffuse;
			float4 color : COLOR;
			
			float4 screenPos;
			float3 viewDir;
			float3 worldRefl;
			float3 worldNormal;
			float3 worldPos;
			INTERNAL_DATA
		};
		
		float _EdgeLength;
		float _Parallax;
		
		float _GamaCorrection;

		float4 tessEdge (appdata v0, appdata v1, appdata v2)
		{
			return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength, _Parallax * 1.5f );
		}

		void disp (inout appdata v)
		{
			float2 UV = v.texcoord.xy * _Tiling.xy + _Tiling.zw;
			float d = ( tex2Dlod(_Displacement, float4(UV,0,0)).x - 0.5 ) * _Parallax * ( 1.0 / _Tiling.x );
			v.vertex.xyz += v.normal * d;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			float2 UV = IN.uv_Diffuse * _Tiling.xy + _Tiling.zw;
			
			half3 texDiffuse = tex2D (_Diffuse, UV).xyz;
			half3 texNormal = tex2D(_Normal,UV).xyz;
			half3 texMetallic = tex2D(_Metallic,UV).xyz;
			half texSmoothness = tex2D(_Smoothness,UV).x;
			half texAO = tex2D(_AO,UV).x;
			half texEdge = tex2D(_Edge,UV).x;
			
			texSmoothness = saturate( _Shininess * texSmoothness );

			texNormal.xyz = texNormal.xyz * 2.0 - 1.0;
			texNormal.y *= -1.0;
			
			float fresnel = 1.0 - saturate( dot( normalize( IN.viewDir.xyz ), texNormal.xyz ) );
			
			texDiffuse.xyz = pow( texDiffuse.xyz, 1.0 - saturate( saturate( _Dust ) * fresnel ) );
			//texDiffuse.xyz *= lerp( 1.0, texAO, _AOPower );
			texDiffuse.xyz *= ( ( texEdge - 0.5 ) * _EdgePower + 1.0 );
			
			texDiffuse.xyz = saturate( texDiffuse.xyz );
			
			o.Albedo = saturate( pow( texDiffuse.rgb, _GamaCorrection ) );
			o.Occlusion = saturate( lerp( 1.0, texAO, _AOPower ) );
			o.Normal = texNormal.xyz;
			o.Metallic = texMetallic.x;
			o.Smoothness = texSmoothness.x;
			o.Alpha = 1.0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
