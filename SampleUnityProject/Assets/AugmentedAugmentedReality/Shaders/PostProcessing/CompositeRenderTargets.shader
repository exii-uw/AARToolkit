Shader "AAR/Blending/CompositeRenderTargets"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            #define CLOSE_TO(x, val) x < val + 0.01 && x > val - 0.01

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _ObjectEnvironmentMaskTex;
            sampler2D _EnvironmentRenderTex;
            float _BlendType;
            int _InvertMainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float2 uvMask = i.uv;
                float2 uvMain = i.uv;

                if (_InvertMainTex)
                {
                    uvMain.y = 1.0 - uvMain.y;
                }

                // sample the texture
                fixed4 col = tex2D(_MainTex, uvMain);
                fixed4 mask = tex2D(_ObjectEnvironmentMaskTex, uvMask);
                fixed4 env = tex2D(_EnvironmentRenderTex, uvMask);

                //// apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);


                // Default blend: Projector 0
                float blend = mask.x;
                
                // Hololens Blend: 1
                if (CLOSE_TO(_BlendType, 1))
                {
                    blend = mask.y;
                }

                return float4(lerp(env.xyz, col.xyz, blend), col.a);
            }
            ENDCG
        }
    }
}
