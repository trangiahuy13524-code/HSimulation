Shader "Custom/TerrainMaskBlend"
{
    Properties
    {
        _GroundTex ("Ground Texture", 2D) = "white" {}
        _WaterTex ("Water Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}

        _GroundTiling ("Ground Tiling", Float) = 10
        _WaterTiling ("Water Tiling", Float) = 10
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_GroundTex);
            SAMPLER(sampler_GroundTex);

            TEXTURE2D(_WaterTex);
            SAMPLER(sampler_WaterTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            
            CBUFFER_START(UnityPerMaterial)
                float _GroundTiling;
                float _WaterTiling;
            CBUFFER_END
            

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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Mask controls terrain
                float mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r;

                // Tile textures independently
                float4 ground =
                    SAMPLE_TEXTURE2D(_GroundTex, sampler_GroundTex, uv * _GroundTiling);

                float4 water =
                    SAMPLE_TEXTURE2D(_WaterTex, sampler_WaterTex, uv * _WaterTiling);

                // Blend
                float4 finalColor = lerp(ground, water, mask);

                return finalColor;
            }

            ENDHLSL
        }
    }
}