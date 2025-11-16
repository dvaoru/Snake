Shader "Custom/TwoColorPulse"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1, 0, 0, 1)
        _ColorB ("Color B", Color) = (0, 0, 1, 1)

        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Float) = 1.0

        _Speed ("Pulse Speed", Float) = 1.0
        _Sharpness ("Sharpness", Range(0, 5)) = 1.0
        _Intensity ("Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _ColorA;
            float4 _ColorB;
            sampler2D _NoiseTex;
            float _NoiseScale;
            float _Speed;
            float _Sharpness;
            float _Intensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // читаем шум
                float noise = tex2D(_NoiseTex, i.uv * _NoiseScale).r;

                // пульсация по времени, чтобы очаги менялись
                float t = _Time.y * _Speed;

                // создаём вспыхивающие очаги
                float pulse = sin(t + noise * 6.28);

                // приводим синус из [-1..1] в [0..1]
                pulse = pulse * 0.5 + 0.5;

                // управляем резкостью очага
                pulse = pow(pulse, _Sharpness);

                // смешивание двух цветов
                float3 col = lerp(_ColorA.rgb, _ColorB.rgb, pulse * _Intensity);

                return float4(col, 1);
            }
            ENDCG
        }
    }
}
