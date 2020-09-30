// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Diffuse_AO_Mix"
{
	Properties
	{
		_Diffuse_Map("Diffuse_Map", 2D) = "white" {}
		_AO_Map("AO_Map", 2D) = "white" {}
		_AOAmount("AO Amount", Float) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Diffuse_Map;
		uniform float4 _Diffuse_Map_ST;
		uniform sampler2D _AO_Map;
		uniform float4 _AO_Map_ST;
		uniform float _AOAmount;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Diffuse_Map = i.uv_texcoord * _Diffuse_Map_ST.xy + _Diffuse_Map_ST.zw;
			o.Albedo = tex2D( _Diffuse_Map, uv_Diffuse_Map ).rgb;
			float2 uv_AO_Map = i.uv_texcoord * _AO_Map_ST.xy + _AO_Map_ST.zw;
			o.Occlusion = pow( tex2D( _AO_Map, uv_AO_Map ).r , _AOAmount );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16301
612;638;752;362;1074.069;186.084;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;4;-654.1684,-18.38394;Float;False;Property;_AOAmount;AO Amount;2;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-774.7679,-214.6149;Float;True;Property;_AO_Map;AO_Map;1;0;Create;True;0;0;False;0;390f24b6633f86b42aea57b4a56ce72b;390f24b6633f86b42aea57b4a56ce72b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;5;-374.6689,-173.084;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-802.9938,136.1047;Float;True;Property;_Diffuse_Map;Diffuse_Map;0;0;Create;True;0;0;False;0;d7621594f8fd9d64e8aa19e123780f68;d7621594f8fd9d64e8aa19e123780f68;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;31,-176;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Diffuse_AO_Mix;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;2;1
WireConnection;5;1;4;0
WireConnection;0;0;1;0
WireConnection;0;5;5;0
ASEEND*/
//CHKSM=CEE2DBF7243C96559B33F52EACBDE644C9897353