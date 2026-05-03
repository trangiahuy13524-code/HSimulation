Shader "Game/TerrainTiles"
{
    Properties
    {
        _MainTex ("Main texture", 2D) = "white" {}
        _UVScale ("UV Scale", Float) = 8 // 512x512 per ... tiles
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _UVScale)
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
                float2 uv  : TEXCOORD0;
                fixed4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                float scale = UNITY_ACCESS_INSTANCED_PROP(Props, _UVScale);

                //scale = max(scale, 0.0001); // safety

                o.uv = v.vertex.xy / scale;

                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 texcol =
                    tex2D(_MainTex, i.uv) * i.color.a;

                return texcol;
            }
            ENDCG
        }
    }
}