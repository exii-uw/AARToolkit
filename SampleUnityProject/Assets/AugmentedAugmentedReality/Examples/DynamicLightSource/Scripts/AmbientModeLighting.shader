Shader "AmbientMoodLighting"
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

            float4 _Color1;
            float4 _Color2;
            float _Speed;

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
                float2 uv = i.uv - float2(sin(_Time.w*_Speed) /3, sin(_Time.w * _Speed)/3);
                float4 c1 = lerp(_Color1, _Color2, (uv.x*uv.x + uv.y*uv.y)/2);

                return c1;
            }
            ENDCG
        }
    }
}
