Shader "Game/WaterTerrainTiles"
{
    Properties
    {
        _MainTex ("Main texture", 2D) = "white" {}

        _WaterColor ("Water Color", Color) = (0.3,0.6,0.8,1)
        _WaveSpeed ("Wave Speed", Float) = 0.15
        _WaveStrength ("Wave Strength", Float) = 0.02
        _WaveScale ("Wave Scale", Float) = 4
    }

    SubShader
    {
        ZWrite Off
        Tags { "Queue"="Transparent" }
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 _WaterColor;
            float _WaveSpeed;
            float _WaveStrength;
            float _WaveScale;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float2 worldUV : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                // world space tiling (NO SEAMS)
                o.worldUV = worldPos.xy / 10;

                o.uv = o.worldUV;

                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float t = _Time.y * _WaveSpeed;

                // --- fake waves ---
                float wave =
                    sin((i.worldUV.x + t) * _WaveScale) +
                    cos((i.worldUV.y - t) * _WaveScale);

                float2 distortedUV =
                    i.uv + wave * _WaveStrength;

                half4 tex = tex2D(_MainTex, distortedUV);

                // water tint
                tex.rgb *= _WaterColor.rgb;

                // vertex alpha blending (your system)
                tex *= i.color.a;

                return tex;
            }

            ENDCG
        }
    }
}