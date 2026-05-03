Shader "Game/WaterTerrainTiles"
{
    Properties
    {
        _MainTex ("Main texture", 2D) = "white" {}
        _WorldUVScale ("World UV Scale", Float) = 8

        _WaterColor ("Water Color", Color) = (0.3,0.6,0.8,1)
        _WaveSpeed ("Wave Speed", Float) = 0.15
        _WaveStrength ("Wave Strength", Float) = 0.02
        _WaveScale ("Wave Scale", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 _WaterColor;
            float _WaveSpeed;
            float _WaveStrength;
            float _WaveScale;

            // INSTANCED PROPERTY
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _WorldUVScale)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldUV : TEXCOORD1;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                // read instanced value
                float scale =
                    UNITY_ACCESS_INSTANCED_PROP(Props, _WorldUVScale);

                // safety against zero
                scale = max(scale, 0.0001);

                o.worldUV = worldPos.xy / scale;
                o.uv = o.worldUV;

                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float t = _Time.y * _WaveSpeed;

                float wave =
                    sin((i.worldUV.x + t) * _WaveScale) +
                    cos((i.worldUV.y - t) * _WaveScale);

                float2 distortedUV =
                    i.uv + wave * _WaveStrength;

                half4 tex = tex2D(_MainTex, distortedUV);

                tex.rgb *= _WaterColor.rgb;
                tex *= i.color.a;

                return tex;
            }
            ENDCG
        }
    }
}