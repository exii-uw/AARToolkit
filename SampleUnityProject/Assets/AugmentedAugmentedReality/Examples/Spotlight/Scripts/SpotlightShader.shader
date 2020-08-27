Shader "SpotlightShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../../Shaders/StaticTextureRendering/StaticTextureRendering.cginc"

            float _Radius = 0.05;
            float4 _SpotlightColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x = uv.x * _ProjectorAspectRatio;
                uv = uv - float2(_ProjectorAspectRatio/2.0f, 0.5f);

                if (abs(uv.x*uv.x + uv.y*uv.y) < _Radius * _Radius)
                {
                    return _SpotlightColor;
                }

                return _BackgroundColor;
            }
            ENDCG
        }
    }
}
