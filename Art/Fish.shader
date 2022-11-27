Shader "Fish"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("Base Color", color) = (1,1,1,1)
        _OutlineWidth("Outline Width", float) = 0.5
        [HDR]_OutlineColor("Outline Color", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float _OutlineWidth;
            float4 _OutlineColor;
        
            sampler2D _MainTex;
            float4 _BaseColor;
            float4 _MainTex_ST;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"    
            }
            Cull back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
                float4 tangent  : TANGENT;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float NdotL = dot(i.normalWS, _MainLightPosition.xyz) * 0.5f + 0.5f;
				float3 light = saturate(floor(NdotL * 3) / (2 - 0.5)) * _MainLightColor.rgb;
                
                float4 col = tex2D(_MainTex, i.uv);
                return half4((col * _BaseColor).rgb * (light + unity_AmbientSky.rgb), 1.0f);
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode"="Outline"    
            }
            Cull front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 uv2          : TEXCOORD2;
            };

            struct v2f
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

                float3 normalVS = mul((float3x3)UNITY_MATRIX_IT_MV, v.normalOS.xyz);
                normalVS = normalize(normalVS);
                float3 ndcNormal = normalize(TransformWViewToHClip(normalVS.xyz)).xyz * o.positionCS.w;
                float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
                float2 offset = lerp(float2(ndcNormal.x * aspect, ndcNormal.y), float2(ndcNormal.x, ndcNormal.y / aspect), step(1.0, aspect));
                
                o.positionCS.xy += offset * _OutlineWidth * 0.007f * v.color.r;
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
    }
}