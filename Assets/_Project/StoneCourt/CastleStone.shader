Shader "JuiceStudy/CastleStone"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _StoneTint ("Stone Tint", Color) = (0.55, 0.68, 0.64, 1)
        _MetalTint ("Aged Brass", Color) = (0.68, 0.52, 0.28, 1)
        _SpriteRect ("Local Sprite Min / Inverse Size", Vector) = (-0.5, -0.5, 1, 1)
        _PhysicalSize ("World Size", Vector) = (1.8, 0.7, 0, 0)
        _Tiles ("Columns / Courses / Stagger", Vector) = (1, 1, 0, 0)
        _Bevel ("Bevel Width", Range(0.01, 0.12)) = 0.055
        _Joint ("Mortar Width", Range(0.002, 0.05)) = 0.012
        _Grain ("Stone Grain", Range(0, 0.3)) = 0.14
        _Emblem ("Braces and Carving", Float) = 1
        _Crenels ("Battlement Notches", Float) = 0
        _Seed ("Stone Variation", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"
               "DisableBatching"="True" "CanUseSpriteAtlas"="False" }
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
                half4 _StoneTint;
                half4 _MetalTint;
                float4 _SpriteRect;
                float4 _PhysicalSize;
                float4 _Tiles;
                float _Bevel, _Joint, _Grain, _Emblem, _Crenels, _Seed;
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
            float StoneHash(float2 p)
            {
                float3 q = frac(float3(p.x, p.y, p.x) * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }
            float StoneNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(StoneHash(cell), StoneHash(cell + float2(1, 0)), f.x),
                            lerp(StoneHash(cell + float2(0, 1)), StoneHash(cell + float2(1, 1)), f.x), f.y);
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = saturate(input.uv);
                // Only the unreachable upper part of the top wall receives notches.
                float notch = step(0.56, frac(uv.x * 12.0)) * step(0.43, uv.y) * _Crenels;
                clip(0.5 - notch);
                float row = floor(uv.y * _Tiles.y);
                float2 grid = float2(uv.x * _Tiles.x + fmod(row, 2.0) * _Tiles.z, uv.y * _Tiles.y);
                float2 cell = floor(grid);
                float2 q = frac(grid);
                // Keep the outermost top/right vertices in their final cell rather than wrapping.
                if (uv.y >= 0.99999) q.y = 1.0;
                if (uv.x >= 0.99999 && _Tiles.z == 0.0) q.x = 1.0;
                float2 size = max(_PhysicalSize.xy / max(_Tiles.xy, 1.0), 0.001);
                float2 edges = min(q, 1.0 - q) * size;
                float edge = min(edges.x, edges.y);
                float seed = _Seed + StoneHash(cell) * 19.0;
                float2 grainUV = uv * _PhysicalSize.xy + seed;
                float grain = StoneNoise(grainUV * 24.0) * 0.65 + StoneNoise(grainUV * 63.0) * 0.35 - 0.5;
                float variation = (StoneHash(cell + _Seed) - 0.5) * 0.14;
                half3 color = _StoneTint.rgb * (0.83 + 0.20 * q.y + 0.08 * (1.0 - q.x) + variation + grain * _Grain);
                float bevel = max(_Bevel, 0.001);
                float left = saturate(1.0 - q.x * size.x / bevel);
                float top = saturate(1.0 - (1.0 - q.y) * size.y / bevel);
                float right = saturate(1.0 - (1.0 - q.x) * size.x / bevel);
                float bottom = saturate(1.0 - q.y * size.y / bevel);
                color += (top * 0.16 + left * 0.09) * _StoneTint.rgb;
                color *= 1.0 - bottom * 0.43 - right * 0.24;
                float face = smoothstep(_Joint, _Joint + 0.012, edge);
                color = lerp(half3(0.035, 0.045, 0.05), color, face);

                float2 p = (q - 0.5) * size;
                float straps = 1.0 - smoothstep(0.018, 0.027, min(abs(q.x - 0.12), abs(q.x - 0.88)) * size.x);
                float metalShade = 0.73 + q.y * 0.38 + grain * 0.1;
                float emblem = 1.0 - smoothstep(0.008, 0.016, abs(abs(p.x) + abs(p.y) * 1.5 - 0.125));
                float decoration = max(straps, emblem) * smoothstep(0.025, 0.06, edge) * _Emblem;
                color = lerp(color, _MetalTint.rgb * metalShade, decoration);
                float2 rivetP = float2(min(abs(q.x - 0.12), abs(q.x - 0.88)) * size.x,
                    min(abs(q.y - 0.24), abs(q.y - 0.76)) * size.y);
                float rivet = 1.0 - smoothstep(0.017, 0.025, length(rivetP));
                color = lerp(color, _MetalTint.rgb * 1.28, rivet * _Emblem);
                return half4(saturate(color), _StoneTint.a);
            }
            ENDHLSL
        }
    }
}
