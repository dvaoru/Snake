Shader "Custom/ChromaFlow"
{
    Properties
    {
        _MainTex ("Albedo (RGB) + Alpha Mask", 2D) = "white" {}
        _NoiseTex ("Noise (grayscale)", 2D) = "gray" {}
        _BaseColor ("Base Tint", Color) = (1,1,1,1)

        _HueSpeed ("Hue Speed (cycles/sec)", Float) = 0.1
        _HueOffset ("Global Hue Offset", Range(0,1)) = 0
        _NoiseStrength ("Noise Influence", Range(0,1)) = 0.4
        _NoiseScale ("Noise Scale", Float) = 1.0

        _SpatialFactor ("Spatial Influence (uv/world)", Float) = 1.0
        _Saturation ("Saturation Multiplier", Float) = 1.0
        _Value ("Brightness (V) Multiplier", Float) = 1.0
        _Intensity ("Overall Intensity", Float) = 1.0

        _CullMode ("Cull Mode (0 Off, 1 Front, 2 Back)", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        LOD 200

        Pass
        {
            Name "FORWARD"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _BaseColor;

            float _HueSpeed;
            float _HueOffset;
            float _NoiseStrength;
            float _NoiseScale;
            float _SpatialFactor;
            float _Saturation;
            float _Value;
            float _Intensity;
            float _CullMode;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
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
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // --- RGB <-> HSV helpers (range H:0..1, S:0..1, V:0..1) ---
            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.w, K.z), float4(c.gb, K.x, K.y), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1e-10;
                float h = abs(q.z + (q.w - q.y) / (6.0 * d + e));
                float s = d / (q.x + e);
                float v = q.x;
                return float3(h, s, v);
            }

            float3 HSVtoRGB(float3 c)
            {
                float h = c.x, s = c.y, v = c.z;
                float3 rgb = clamp(abs(frac(h + float3(0.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
                rgb = lerp(float3(1.0,1.0,1.0), rgb, s);
                return rgb * v;
            }

            // Sample grayscale noise (if no noise texture, will be 0.5)
            float SampleNoise(float2 uv)
            {
                #if UNITY_NO_DXT5nm // safe fallback
                float4 n = tex2D(_NoiseTex, uv * _NoiseScale);
                return n.r;
                #else
                float4 n = tex2D(_NoiseTex, uv * _NoiseScale);
                return n.r;
                #endif
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // base texture and mask
                fixed4 albedo = tex2D(_MainTex, i.uv) * _BaseColor;
                float mask = albedo.a; // use alpha as mask (1=visible)

                // time-driven hue shift (in cycles; we use unity_Time.y for time in seconds)
                float time = _Time.y;

                // spatial coord for per-pixel variation
                float2 spatialUV = i.uv * _SpatialFactor + i.worldPos.xz * 0.001; // small world-pos influence

                // sample noise (0..1)
                float noise = SampleNoise(spatialUV);

                // compute hue offset: global offset + time + noise contribution + spatial flow
                float hueFlow = _HueOffset + time * _HueSpeed + (noise - 0.5) * _NoiseStrength;
                // optionally add spatial wave for more movement
                hueFlow += sin((spatialUV.x + spatialUV.y + time * _HueSpeed * 0.5) * 6.28318) * 0.05;

                // convert albedo to HSV
                float3 hsv = RGBtoHSV(albedo.rgb);

                // apply hue shift and saturation/value multipliers
                hsv.x = frac(hsv.x + hueFlow);
                hsv.y = saturate(hsv.y * _Saturation);
                hsv.z = saturate(hsv.z * _Value);

                float3 rgb = HSVtoRGB(hsv);

                // mix original and shifted color by intensity
                float3 outColor = lerp(albedo.rgb, rgb, _Intensity);

                // final alpha = mask
                return float4(outColor, mask);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
