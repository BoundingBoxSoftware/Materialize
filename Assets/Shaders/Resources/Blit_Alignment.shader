// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Alignment" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
			
	float2 _PointTL;
	float2 _PointTR;
	float2 _PointBL;
	float2 _PointBR;
	
	float _Lens;
	float _Width;
	float _Height;
	
	float _PerspectiveX;
	float _PerspectiveY;
	
	float _GamaCorrection;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	v2f vert(appdata_img v) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}

	float4 frag_lens (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;
		float aspect = _Width / _Height;
		float2 distortUV = UV;
		distortUV = distortUV - 0.5;
		distortUV.x *= aspect;
		distortUV = distortUV * ( ( 1.0 - cos( distortUV.yx ) ) * _Lens + 1.0 );
		distortUV.x *= 1.0 / aspect;
		distortUV += 0.5;

		
		float2 distortUV3 = UV - 0.5;
		distortUV3.x *= aspect;
		float2 offsetSquared = distortUV3 * distortUV3;
		float radiusSquared = offsetSquared.x + offsetSquared.y;
		distortUV3 *= radiusSquared * _Lens + 1.0;
		distortUV3 *= 1.0 - ( _Lens * 0.5 );
		distortUV3.x *= 1.0 / aspect;
		distortUV3 += 0.5;
		
		half4 c = tex2D (_MainTex, distortUV3);
		

		return float4( c.xyz, 1 );

	}
	
	
	float4 frag_align (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;
		
		float2 newUVtop = lerp( _PointTL, _PointTR, UV.x );
		float2 newUVbot = lerp( _PointBL, _PointBR, UV.x );
		float2 newUV = lerp( newUVbot, newUVtop, UV.y );
		
		half4 c = tex2D (_MainTex, newUV);

		return float4( c.xyz, 1 );

	}
	
	float4 frag_perspective (v2f IN) : SV_Target
	{
	
		float2 UV = IN.uv;
		float2 newUV1 = IN.uv;
		float2 newUV2 = IN.uv;
		
		float perspectiveX1 = 1;
		if( _PerspectiveX > 0 ){
			perspectiveX1 = 1.0 / ( _PerspectiveX + 1.0 );
			newUV1.x = 1.0 - pow( 1.0 - newUV1.x, perspectiveX1 );
		}else{
			perspectiveX1 = 1.0 / ( abs( _PerspectiveX ) + 1.0 );
			newUV1.x = pow( newUV1.x, perspectiveX1 );
		}
		
		float perspectiveX2 = 1;
		if( _PerspectiveX > 0 ){
			perspectiveX2 = _PerspectiveX + 1.0;
			newUV2.x = pow( newUV2.x, perspectiveX2 );
		}else{
			perspectiveX2 = abs( _PerspectiveX ) + 1.0;
			newUV2.x = 1.0 - pow( 1.0 - newUV2.x, perspectiveX2 );
		}
		
		UV.x = lerp( newUV1.x, newUV2.x, 1.0 - IN.uv.x );
		
		float perspectiveY1 = 1;
		if( _PerspectiveY > 0 ){
			perspectiveY1 = 1.0 / ( _PerspectiveY + 1.0 );
			UV.y = 1.0 - pow( 1.0 - UV.y, perspectiveY1 );
		}else{
			perspectiveY1 = 1.0 / ( abs( _PerspectiveY ) + 1.0 );
			UV.y = pow( UV.y, perspectiveY1 );
		}
		
		float perspectiveY2 = 1;
		if( _PerspectiveY > 0 ){
			perspectiveY2 = 1.0 / ( _PerspectiveY + 1.0 );
			UV.y = 1.0 - pow( 1.0 - UV.y, perspectiveY2 );
		}else{
			perspectiveY2 = 1.0 / ( abs( _PerspectiveY ) + 1.0 );
			UV.y = pow( UV.y, perspectiveY2 );
		}
		
		UV.y = lerp( newUV1.y, newUV2.y, 1.0 - IN.uv.y );

		half4 c = tex2D (_MainTex, UV);

		return float4( c.xyz, 1 );

	}	
	
	ENDCG
		
	SubShader {
	
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_lens
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_align
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_perspective
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		} 
		 
	} 
	
	Fallback off
}
