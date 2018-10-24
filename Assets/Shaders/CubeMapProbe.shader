Shader "Custom/CubeMapProbe" {
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
				float3 localNormal : TEXCOORD0;
			};

			// vertex shader
			v2f vert (appdata_full v) {
				v2f o;
				o.pos = UnityObjectToClipPos ( v.vertex );
				o.localNormal = v.normal.xyz;
				return o;
			}
			
			// fragment shader
			fixed4 frag (v2f IN) : SV_Target {
				
				fixed3 localNormal = normalize( IN.localNormal.xyz );
				
				half3 ambIBL = texCUBElod(_GlobalCubemap, half4( localNormal , 0.0 ) ).xyz;
				
				return float4( ( ambIBL + ( ambIBL * ambIBL ) ) * _Factor, 1.0 );
				
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
