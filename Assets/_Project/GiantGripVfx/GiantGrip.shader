Shader "JuiceStudy/GiantGripVfx"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Mode ("Aura 0 / Rune 1 / Mote 2", Float) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _BodySize ("Physical Paddle Size", Vector) = (4.8, 0.5, 0, 0)
        _QuadSize ("Aura Quad Size", Vector) = (5, 0.7, 0, 0)
        _Age ("Scaled Active Age", Float) = 0
        _Pulse ("Activation Pulse", Range(0, 1)) = 0
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
                float4 _BodySize, _QuadSize;
                float _Mode, _Age, _Pulse;
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
                    float2 p = (uv - 0.5) * max(_QuadSize.xy, float2(0.001, 0.001));
                    float2 q = abs(p) - max(_BodySize.xy * 0.5 - 0.055, float2(0.001, 0.001));
                    float distance = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - 0.055;
                    float edge = 1.0 - smoothstep(0.008, 0.055, abs(distance));
                    float halo = (1.0 - smoothstep(0.015, 0.10, max(distance, 0.0))) * smoothstep(-0.05, -0.01, distance);
                    float flow = pow(saturate(0.5 + 0.5 * cos(uv.x * 25.1327 - _Age * 7)), 6.0);
                    float mask = max(edge * (0.65 + flow * 0.25), halo * (0.12 + _Pulse * 0.20));
                    color *= lerp(half3(0.26, 0.92, 0.65), half3(1, 0.86, 0.47), _Pulse * 0.7);
                    alpha *= saturate(mask);
                }
                else if (_Mode < 1.5)
                {
                    float2 p = uv * 2.0 - 1.0;
                    float diamond = abs(abs(p.x) + abs(p.y) * 0.8 - 0.65);
                    float carving = 1.0 - smoothstep(0.07, 0.14, diamond);
                    float stem = (1.0 - smoothstep(0.08, 0.18, abs(p.x))) * (1.0 - smoothstep(0.78, 0.95, abs(p.y)));
                    alpha *= max(carving, stem * 0.75) * (0.8 + _Pulse * 0.2);
                }
                else
                {
                    float2 p = uv * 2.0 - 1.0;
                    float diamond = abs(p.x) + abs(p.y);
                    alpha *= 1.0 - smoothstep(0.50, 0.95, diamond);
                }
                clip(alpha - 0.002);
                return half4(saturate(color), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
