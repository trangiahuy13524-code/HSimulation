Shader "Custom/OtherOutline"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Range(0,8)) = 1
        _OutlineEnabled ("Outline", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _OutlineColor;

            float _OutlineSize;
            float _OutlineEnabled;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                if (_OutlineEnabled < 0.5)
                    return c;

                if (c.a > 0.001)
                    return c;

                float2 t = _MainTex_TexelSize.xy * _OutlineSize;

                float alpha = 0;

                alpha = max(alpha, tex2D(_MainTex, i.uv + float2( t.x, 0)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-t.x, 0)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(0,  t.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(0, -t.y)).a);

                alpha = max(alpha, tex2D(_MainTex, i.uv + float2( t.x,  t.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-t.x,  t.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2( t.x, -t.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-t.x, -t.y)).a);

                if (alpha > 0.001)
                    return _OutlineColor;

                return c;
            }
            ENDCG
        }
    }
}