Shader "JuiceStudy/RelicGear"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Mode ("Orb 0 / Paddle 1 / Halo 2 / Contact Ring 3", Float) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _SpriteRect ("Local Min / Inverse Size", Vector) = (-0.5, -0.5, 1, 1)
        _Size ("Physical Size", Vector) = (1, 1, 0, 0)
        _Impact ("Contact Emphasis", Range(0, 1)) = 0
        _Empowered ("Wide Paddle Rune", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "DisableBatching"="True" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float4 _SpriteRect;
                float4 _Size;
                float _Mode, _Impact, _Empowered;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = (input.positionOS.xy - _SpriteRect.xy) * _SpriteRect.zw;
                return output;
            }
            float RelicHash(float2 p)
            {
                float3 q = frac(float3(p.x, p.y, p.x) * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }
            float RelicNoise(float2 p)
            {
                float2 c = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(RelicHash(c), RelicHash(c + float2(1, 0)), f.x),
                            lerp(RelicHash(c + float2(0, 1)), RelicHash(c + 1), f.x), f.y);
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = saturate(input.uv);
                float2 p = uv * 2.0 - 1.0;
                float radius = length(p);
                float feather = max(fwidth(radius), 0.008);
                half3 color;
                float alpha = _Tint.a;
                if (_Mode < 0.5)
                {
                    float sphereZ = sqrt(saturate(1.0 - dot(p, p)));
                    float light = saturate(dot(float3(p, sphereZ), normalize(float3(-0.4, 0.55, 0.8))));
                    half3 ivory = half3(0.92, 0.98, 0.89) * (0.70 + light * 0.38);
                    float rim = smoothstep(0.76, 0.84, radius);
                    half3 brass = half3(0.83, 0.65, 0.32) * (0.72 + light * 0.45);
                    color = lerp(ivory, brass, rim);
                    float arc = 1.0 - smoothstep(0.018, 0.045, abs(radius - 0.65));
                    float angle = atan2(p.y, p.x);
                    float runes = smoothstep(0.65, 0.90, cos(angle * 8.0 + 0.7));
                    color = lerp(color, half3(0.23, 0.75, 0.62), arc * runes * 0.7);
                    float gleam = pow(saturate(1.0 - length(p - float2(-0.28, 0.32)) * 3.6), 2.0);
                    color = lerp(color, half3(1.0, 1.0, 0.94), gleam * 0.8 + _Impact * 0.55);
                    color *= lerp(0.48, 1.0, 1.0 - smoothstep(0.94, 1.0, radius));
                    alpha *= 1.0 - smoothstep(1.0 - feather, 1.0, radius);
                    color *= _Tint.rgb;
                }
                else if (_Mode < 1.5)
                {
                    float2 size = max(_Size.xy, 0.001);
                    float2 edge = min(uv, 1.0 - uv) * size;
                    // Shallow rounded corners soften the cap silhouette without touching the collider.
                    float corner = min(0.065, size.y * 0.25);
                    float2 inset = abs((uv - 0.5) * size) - (size * 0.5 - corner);
                    float edgeDistance = length(max(inset, 0.0)) + min(max(inset.x, inset.y), 0.0) - corner;
                    alpha *= 1.0 - smoothstep(-max(fwidth(edgeDistance), 0.001), 0.0, edgeDistance);
                    float grain = sin(uv.y * 87 + RelicNoise(uv * float2(7, 13)) * 4.0 + sin(uv.x * 22) * 0.8);
                    half3 wood = half3(0.30, 0.16, 0.075) * (0.92 + uv.y * 0.21 + grain * 0.10);
                    float metal = 1.0 - smoothstep(0.32, 0.35, edge.x);
                    half3 brass = half3(0.71, 0.54, 0.29) * (0.72 + uv.y * 0.40);
                    color = lerp(wood, brass, metal);
                    float bevel = saturate(min(edge.x, edge.y) / 0.055);
                    color *= lerp(0.52, 1.0, bevel);
                    color += (saturate(1 - (1 - uv.y) * size.y / 0.055) * 0.14) * half3(1, 0.83, 0.52);
                    float seam = 1.0 - smoothstep(0.008, 0.017, abs(edge.x - 0.34));
                    color *= 1.0 - seam * 0.55;
                    float2 world = (uv - 0.5) * size;
                    float runeX = min(abs(world.x), min(abs(world.x - 0.53), abs(world.x + 0.53)));
                    float diamond = abs(runeX + abs(world.y) * 1.35 - 0.115);
                    float rune = 1.0 - smoothstep(0.009, 0.018, diamond);
                    float inlay = (1.0 - smoothstep(0.007, 0.015, abs(world.y - 0.11))) * step(0.40, edge.x);
                    half3 jade = lerp(half3(0.27, 0.76, 0.61), half3(0.51, 1.0, 0.69), _Empowered);
                    color = lerp(color, jade * (0.85 + _Empowered * 0.15), saturate(rune + inlay * 0.45));
                    float2 rivet = float2(abs(edge.x - 0.16), world.y);
                    float pin = 1.0 - smoothstep(0.024, 0.038, length(rivet));
                    color = lerp(color, half3(0.94, 0.79, 0.47), pin);
                    color = lerp(color, half3(1.0, 0.91, 0.63), _Impact * 0.48);
                    color *= lerp(half3(1, 1, 1), _Tint.rgb, 0.10);
                }
                else if (_Mode < 2.5)
                {
                    float glow = saturate(1.0 - radius);
                    alpha *= glow * glow;
                    color = _Tint.rgb;
                }
                else
                {
                    float band = 1.0 - smoothstep(0.025, 0.09, abs(radius - 0.78));
                    alpha *= band * (1.0 - smoothstep(0.94, 1.0, radius));
                    color = _Tint.rgb;
                }
                clip(alpha - 0.002);
                return half4(saturate(color), alpha);
            }
            ENDHLSL
        }
    }
}
