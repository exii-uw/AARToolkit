Shader "AAR/Outlines" {
	Properties{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
	}

		SubShader
		{
			Cull Off
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag
				#pragma shader_feature USE_DISTANCE
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float depth : depth;
				};

				float _LineThickness = 0.05;
				float4 _Color;
				float4 _LineColor;
				sampler2D _MainTex;
				float4 _MainTex_TexelSize;

				struct g2f
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float depth : depth;
					float3 barycoord : barycoord;
				};

				[maxvertexcount(3)]
				void geom(triangle v2f input[3], inout TriangleStream<g2f> triStream)
				{
					g2f output;

					[unroll]
					for (int i = 0; i < 3; ++i)
					{
						output.vertex = input[i].vertex;
						output.uv = input[i].uv;
						output.depth = input[i].depth;

						// BarryCentric Coordinates
						float3 coords = float3(0, 0, 0);
						coords[i] = 1;
						output.barycoord = coords;

						triStream.Append(output);
					}
					triStream.RestartStrip();
				}


				v2f vert(appdata v)
				{
					v2f o;
					//o.vertex = v.vertex;
					o.depth = length(v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);

					o.uv = v.uv;
					return o;
				}

				fixed4 frag(g2f i) : SV_Target
				{

					float minBarryCoord = min(i.barycoord.x, min(i.barycoord.y, i.barycoord.z));
					if (minBarryCoord < _LineThickness) {
						return _LineColor;
					}

					return 0;
				}
				ENDCG
			}
		}
}