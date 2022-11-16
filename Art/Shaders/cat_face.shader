Shader "GameOff/cat_face"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TileID("Tile ID", int) = 0
    }
    SubShader
    {
        Tags {  }

        Pass
        {
            Tags
            {
                "Queue"="Transparent"     
                "RenderType"="Transparent"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
                        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TileID;

            void Flipbook(float2 UV, float Width, float Height, float Tile, float2 Invert, out float2 Out)
            {
                Tile = fmod(Tile, Width * Height);
                float2 tileCount = float2(1.0, 1.0) / float2(Width, Height);
                float tileY = abs(Invert.y * Height - (floor(Tile * tileCount.x) + Invert.y * 1));
                float tileX = abs(Invert.x * Width - ((Tile - Width * floor(Tile * tileCount.x)) + Invert.x * 1));
                Out = (UV + float2(tileX, tileY)) * tileCount;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 flipUV = 0;
                Flipbook(i.uv, 4, 4, _TileID,float2(1,1), flipUV);
                fixed4 col = tex2D(_MainTex, flipUV);
                return col;
            }
            ENDCG
        }
    }
}
