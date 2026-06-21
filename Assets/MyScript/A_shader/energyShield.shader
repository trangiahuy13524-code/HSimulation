Shader "Custom/EnergyShield"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (0,1,1,1)

        _EdgePower ("Edge Power", Range(0.1,8)) = 3
        _Glow ("Glow", Range(0,5)) = 2

        // _NoiseTex ("Noise", 2D) = "white" {}
        // _NoiseSpeed ("Noise Speed", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // TEXTURE2D(_NoiseTex);
            // SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _EdgePower;
                float _Glow;
                // float _NoiseSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // animated noise
                // float2 noiseUV = uv + _Time.y * _NoiseSpeed;
                // float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                float4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // fake fresnel edge
                float2 centerUV = uv * 2 - 1;
                float edge = 1 - length(centerUV);

                edge = pow(saturate(edge), _EdgePower);

                float glow = (1 - edge) * _Glow;

                float alpha = glow * sprite.a;

                float3 col = _Color.rgb * glow;

                return float4(col, alpha);
            }
            ENDHLSL
        }
    }
}