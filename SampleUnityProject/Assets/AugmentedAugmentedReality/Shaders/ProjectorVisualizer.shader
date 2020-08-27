Shader "AAR/ProjectorVisualizer"
{
	Properties{
		_ShadowTex("Cookie", 2D) = "gray" {}
		_Color("Color", Color) = (1,1,1,1)
		_Border("Border Thickness",  Range(0, 1)) = 0.02
		_Alpha("Alpha",  Range(0, 1)) = 1
		_GrayCutOff("Gray Cutoff",  Range(0, 1)) = 0.00
	}
		Subshader{
			Tags {"Queue" = "Transparent"}
			Pass {
				ZWrite Off
				ColorMask RGB
				Blend OneMinusSrcAlpha SrcAlpha
				Offset -1, -1

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

				struct v2f {
					float4 uvShadow : TEXCOORD0;
					float4 uvFalloff : TEXCOORD1;
					UNITY_FOG_COORDS(2)
					float4 pos : SV_POSITION;
				};

				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;

				v2f vert(float4 vertex : POSITION)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(vertex);
					o.uvShadow = mul(unity_Projector, vertex);
					o.uvFalloff = mul(unity_ProjectorClip, vertex);
					UNITY_TRANSFER_FOG(o,o.pos);
					return o;
				}

				sampler2D _ShadowTex;
				fixed4 _Color;
				float _Border;
				float _Alpha;
				float _GrayCutOff;


				fixed4 frag(v2f i) : SV_Target
				{
					float4 uv = UNITY_PROJ_COORD(i.uvShadow);
					uv = uv / uv.w; // project uv
					uv.y = 1.0 - uv.y;

					fixed4 texS = tex2D(_ShadowTex, uv.xy);

					// Remove all samles outside area
					clip(uv.xy);
					clip(float2(1, 1) - uv.xy);

					// Give border
					if (uv.x < _Border || uv.x > 1 - _Border || uv.y < _Border || uv.y > 1 - _Border)
						return float4(_Color.xyz, 0);

					float alpha = 1 - _Alpha;
					if (texS.x < _GrayCutOff && texS.y < _GrayCutOff && texS.z < _GrayCutOff)
						alpha = 1;

					return float4(texS.xyz, alpha);
				}
				ENDCG
			}
		}
}
