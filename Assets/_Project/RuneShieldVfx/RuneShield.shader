Shader "JuiceStudy/RuneShieldVfx"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Mode ("Ward 0 / Anchor 1 / Shard 2", Float) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Width ("Ward Width", Float) = 14.85
        _Age ("Scaled Ward Age", Float) = 0
        _Pulse ("Observed Deflection Pulse", Range(0, 1)) = 0
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
                float _Mode, _Width, _Age, _Pulse;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Tint;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = saturate(input.uv);
                half3 color = input.color.rgb;
                float alpha = input.color.a;
                if (_Mode < 0.5)
                {
                    // The quad's lower edge stays on the existing shield plane. Runes rise above the hotbar.
                    float x = (uv.x - 0.5) * max(_Width, 0.001);
                    float height = uv.y * 0.46;
                    float baseline = 1.0 - smoothstep(0.012, 0.05, height);
                    float cellX = (frac(x / 0.90 + 0.5) - 0.5) * 0.90;
                    float runeDistance = abs(abs(cellX) + abs(height - 0.30) * 1.25 - 0.07);
                    float runes = (1.0 - smoothstep(0.008, 0.017, runeDistance)) * (1.0 - smoothstep(0.10, 0.12, abs(cellX)));
                    float rail = 1.0 - smoothstep(0.004, 0.012, abs(height - 0.30));
                    float weaveX = abs(frac(x * 1.4 + height * 4 + _Age * 0.025) - 0.5);
                    float weaveY = abs(frac(x * 1.4 - height * 4 - _Age * 0.025) - 0.5);
                    float weave = max(1.0 - smoothstep(0.012, 0.035, weaveX), 1.0 - smoothstep(0.012, 0.035, weaveY));
                    float fade = 1.0 - smoothstep(0.18, 0.46, height);
                    float wave = 0.5 + 0.5 * cos(x * 4.5 - _Age * 48);
                    float mask = max(baseline * 0.9, max(runes * 0.90, rail * 0.12));
                    mask = max(mask, (0.025 + weave * 0.055 + _Pulse * wave * 0.12) * fade);
                    color *= lerp(half3(0.34, 0.93, 0.78), half3(1.0, 0.85, 0.43), _Pulse);
                    alpha *= saturate(mask) * (0.45 + 0.55 * smoothstep(0.0, 0.18, _Age));
                }
                else if (_Mode < 1.5)
                {
                    float2 p = uv * 2.0 - 1.0;
                    float diamond = abs(p.x) * 1.15 + abs(p.y);
                    float shape = 1.0 - smoothstep(0.77, 0.95, diamond);
                    float edge = smoothstep(0.52, 0.76, diamond);
                    color *= lerp(half3(0.28, 0.80, 0.68), half3(0.94, 0.84, 0.50), saturate(edge + _Pulse * 0.75));
                    float carving = 1.0 - smoothstep(0.035, 0.07, abs(abs(p.x) - abs(p.y) * 0.32));
                    color = lerp(color, half3(1.0, 0.95, 0.72), carving * 0.65);
                    alpha *= shape * (0.72 + _Pulse * 0.28);
                }
                else
                {
                    float2 p = uv * 2.0 - 1.0;
                    float shape = 1.0 - smoothstep(0.65, 1.0, abs(p.x) + abs(p.y));
                    color *= lerp(half3(0.77, 0.52, 0.18), half3(1.0, 0.95, 0.68), saturate(1.0 - abs(p.x) * 2.0));
                    alpha *= shape;
                }
                clip(alpha - 0.002);
                return half4(saturate(color), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
