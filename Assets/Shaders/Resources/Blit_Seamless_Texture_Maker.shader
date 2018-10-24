// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blit_Seamless_Texture_Maker" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
		
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _HeightTex;
	sampler2D _TargetTex;
	
	float _Falloff;
	float _OverlapX;
	float _OverlapY;
	
	float _SmoothBlend;
	
	float _GamaCorrection;
	
	float _IsSingleChannel;
	float _IsHeight;
	float _IsNormal;
	float _FlipY;

	float _SplatScale;
	float4 _SplatKernel;
	
	float4x4 _SplatMatrix[9];
	float _SplatRotation;
	float _SplatRotationRandom;
	float2 _AspectRatio;
	float2 _MaskOffset;
	float2 _MaskOffset2;
	float3 _ObjectScale;
	float _SplatRandomize;

	float3 _Wobble;

	float2 _TargetAspectRatio;

	static const int OffsetKernelSamples = 9;		
	static const float2 OffsetKernel[OffsetKernelSamples] =
	{
		float2(1,1),
		float2(0,1),
		float2(-1,1),
		float2(1,0),
		float2(0,0),
		float2(-1,0),
		float2(1,-1),
		float2(0,-1),
		float2(-1,-1)
	};

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

	float4 frag (v2f IN) : SV_Target
	{
		
		float2 overlap = float2( _OverlapX, _OverlapY );
		float2 invOverlap = 1.0 - float2( _OverlapX, _OverlapY );
		float2 oneOverOverlap = 1.0 / float2( _OverlapX, _OverlapY );
		
		float2 UV = IN.uv;
		float2 UV2 = UV - float2( overlap.x, 0.0 );
		float2 UV3 = UV - float2( 0.0, overlap.y );
		float2 UV4 = UV - float2( overlap.x, overlap.y );
		
		float2 UVMask = saturate( (  1.0 - frac( UV + 0.0 ) - invOverlap ) * oneOverOverlap );
		
		UV = frac( UV );
		UV2 = frac( UV2 );
		UV3 = frac( UV3 );
		UV4 = frac( UV4 );
		
		UV *= invOverlap;
		
		UV2.x += overlap.x;
		UV2 *= invOverlap;
		
		UV3.y += overlap.y;
		UV3 *= invOverlap;
		
		UV4 += overlap;
		UV4 *= invOverlap;
		
		half heightTex = tex2Dlod(_HeightTex, float4( UV, 0, 0 ) ).x;
		half heightTex2 = tex2Dlod(_HeightTex, float4( UV2, 0, 0 ) ).x;
		half heightTex3 = tex2Dlod(_HeightTex, float4( UV3, 0, 0 ) ).x;
		half heightTex4 = tex2Dlod(_HeightTex, float4( UV4, 0, 0 ) ).x;
		
		half4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
		half4 mainTex2 = tex2Dlod(_MainTex, float4( UV2, 0, 0 ) );
		half4 mainTex3 = tex2Dlod(_MainTex, float4( UV3, 0, 0 ) );
		half4 mainTex4 = tex2Dlod(_MainTex, float4( UV4, 0, 0 ) );

		half SSHigh =  0.01 + ( 0.5 * saturate( _Falloff ) );
		half SSLow =  -0.01 - ( 0.5 * saturate( _Falloff ) );
		half TexBlend = smoothstep( SSLow, SSHigh, ( heightTex2 + UVMask.x ) - ( heightTex + ( 1.0 - UVMask.x ) ) );
		
		
		half4 blendTexH = lerp( mainTex, mainTex2, UVMask.x );
		half heightTexH = max( heightTex + ( 1.0 - UVMask.x ), heightTex2 + UVMask.x ) - 1.0;
		heightTexH += saturate( min( UVMask.x, ( 1.0 - UVMask.x ) ) );
		half4 mainTexH = lerp( mainTex, mainTex2, TexBlend );
		
		TexBlend = smoothstep( SSLow, SSHigh, ( heightTex4 + UVMask.x ) - ( heightTex3 + ( 1.0 - UVMask.x ) ) );
		
		half4 blendTexV = lerp( mainTex3, mainTex4, UVMask.x );
		half heightTexV = max( heightTex3 + ( 1.0 - UVMask.x ), heightTex4 + UVMask.x ) - 1.0;
		heightTexV += saturate( min( UVMask.x, ( 1.0 - UVMask.x ) ) );
		half4 mainTexV = lerp( mainTex3, mainTex4, TexBlend );
		
		TexBlend = smoothstep( SSLow, SSHigh, ( heightTexV + UVMask.y ) - ( heightTexH + ( 1.0 - UVMask.y ) ) );
		
		half4 blendTex = lerp( mainTexH, mainTexV, UVMask.y );
		heightTex = max( heightTexH + ( 1.0 - UVMask.y ), heightTexV + UVMask.y ) - 1.0;
		heightTex += saturate( min( UVMask.y , ( 1.0 - UVMask.y ) ) );
		mainTex = lerp( mainTexH, mainTexV, TexBlend );	
		
		if( _IsHeight > 0.5 ){
			return half4( heightTex.xxx, 1.0 );
		} else {
			return half4( mainTex.xyz, 1.0 );
		}

	}
	
	float4 frag_splat (v2f IN) : SV_Target
	{
		
		float2 overlap = float2( _OverlapX, _OverlapY );
		float2 invOverlap = 1.0 - float2( _OverlapX, _OverlapY );
		float2 oneOverOverlap = 1.0 / float2( _OverlapX, _OverlapY );
		
		float4 targetTex = tex2Dlod( _TargetTex, float4(IN.uv,0,0) );
		targetTex.w = ( 1.0 / targetTex.w ) - 1.0;
		float4 tempTex = float4(0,0,0,0);
		
		for( int i = 0; i < OffsetKernelSamples; i++ ){

			float2 localPos = ( IN.uv - _SplatKernel.xy + OffsetKernel[i].xy) * ( 1.0 / ( _SplatScale * _SplatKernel.z ) ) * _TargetAspectRatio;

			float rotation = _SplatRotation * -6.28318530718;
			rotation += _SplatRotationRandom * _SplatRandomize * -6.28318530718;
			float2 tempPos = localPos;
			localPos.x = cos( rotation ) * tempPos.x - sin( rotation ) * tempPos.y;
			localPos.y = sin( rotation ) * tempPos.x + cos( rotation ) * tempPos.y;

			float2 localMaskPos = localPos * 2.0;
			float CenterMask = pow( saturate( ( ( 1.0 - saturate( abs( localMaskPos.x ) ) ) * ( 1.0 - saturate( abs( localMaskPos.y ) ) ) - 0.1 ) * 2.0 ), 0.3 );
			float UVMask = saturate( ( 1.0 - saturate( abs( localMaskPos.x ) ) ) * ( 1.0 - saturate( abs( localMaskPos.y ) ) ) * 10.0 );

			localPos *= _AspectRatio.yx;
			localPos *= ( 1.0 / (_Wobble.z + 1.0) );
			localPos += _Wobble.xy * _Wobble.z;
			localPos += 0.5;

			half heightTex = tex2Dlod(_HeightTex, float4( localPos.xy, 0, 0 ) ).x;
			half4 thisTex = tex2Dlod(_MainTex, float4( localPos.xy, 0, 0 ) );
			
			half SSHigh =  0.01 + ( 0.5 * saturate( _Falloff ) );
			half SSLow =  -0.01 - ( 0.5 * saturate( _Falloff ) );
			if( _IsHeight > 0.5 ){
				SSHigh =  0.01 + 0.25;
				SSLow =  -0.01 - 0.25;
			}
			
			if( _IsNormal > 0.5 ){				
				float3 tempTex = thisTex.xyz * 2.0 - 1.0;
				

				if( _FlipY > 0.5 ){
					rotation *= -1.0;
				}
				
				thisTex.x = cos( rotation ) * tempTex.x - sin( rotation ) * tempTex.y;
				thisTex.y = sin( rotation ) * tempTex.x + cos( rotation ) * tempTex.y;
				//thisTex.y *= -1.0;
				thisTex.xy = thisTex.xy * 0.5 + 0.5;
			}


			half thisHeight = ( heightTex.x + 0.2 ) * CenterMask * UVMask;
			half TexBlend = smoothstep( SSLow, SSHigh, targetTex.w - thisHeight );
			targetTex.xyz = lerp( thisTex.xyz, targetTex.xyz, TexBlend );
			targetTex.w = max( targetTex.w, thisHeight );

		}
		
		targetTex.w = 1.0 / ( targetTex.w + 1.0 );
		
		return targetTex;

	}
	
	float4 frag_clear (v2f IN) : SV_Target
	{
		return float4(0,0,0,1);
	}
	
	float4 frag_transfer (v2f IN) : SV_Target
	{
		half4 mainTex = tex2Dlod(_MainTex, float4( IN.uv, 0, 0 ) );
		return float4(mainTex.xyz,1);
	}
	
	ENDCG
		
	SubShader {
	
		Pass {
			ZTest Always Cull Off ZWrite Off Blend Off
			Fog { Mode off }      

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
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
			#pragma fragment frag_splat
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
			#pragma fragment frag_clear
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
			#pragma fragment frag_transfer
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			ENDCG
		}
		 
	} 
	
	Fallback off
}
