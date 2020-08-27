Shader "StaticMaterial/FlipCamaraImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #include "StaticTextureRendering.cginc"
            
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // Correct uv aspect
                float texAspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                float aspectAdjustment = _ProjectorAspectRatio / texAspect;
                uv.x = uv.x * aspectAdjustment;
                float offset = (aspectAdjustment - 1.0) / 2;
                
                // Flip image
                uv.y = 1 - uv.y;

                // Correct for aspect ratio 
                uv.x = uv.x - offset;
                if (uv.x < 0 || uv.x > 1)
                    return _BackgroundColor;
            
                fixed4 col = tex2D(_MainTex, uv);

                return col;
            }
            ENDCG
        }
    }
}
