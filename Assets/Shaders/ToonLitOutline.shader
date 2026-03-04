Shader "Toon/LitOutline"
{
    Properties
    {
        _Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _Outline("Outline width", Range(0.001, 0.03)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        // ---- BASE PASS (couleurs simples comme BasicOutline) ----
        Pass
        {
            Name "BASE"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                return col;
            }
            ENDHLSL
        }

        // ---- OUTLINE PASS ----
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float _Outline;
            float4 _OutlineColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                
                // Convertir en espace monde
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                
                // Déplacer le long de la normale
                positionWS += normalWS * _Outline;
                
                // Convertir en clip space
                o.positionHCS = TransformWorldToHClip(positionWS);
                
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}