Shader "JuiceStudy/FireballVfx"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Mode ("Flame 0 / Glow 1 / Ribbon 2 / Ring 3", Float) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Age ("Scaled Flight Age", Float) = 0
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
                float _Mode;
                float _Age;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
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
                    // The 0.22-unit bright head matches the existing gameplay radius. Fire trails below it.
                    float2 p = (uv - float2(0.5, 0.67)) * float2(0.60, 0.90);
                    float radius = length(p);
                    float feather = max(fwidth(radius), 0.002);
                    float body = 1.0 - smoothstep(0.206, 0.222 + feather, radius);
                    float trailing = saturate(-p.y / 0.603);
                    float sway = (sin(_Age * 21 + trailing * 15) * 0.018 +
                                  sin(_Age * 33 - trailing * 19) * 0.008) * trailing;
                    float width = 0.195 * pow(saturate(1.0 - trailing), 0.72) *
                                  (0.86 + 0.14 * sin(_Age * 27 + trailing * 18));
                    float across = abs(p.x + sway);
                    float tail = (1.0 - smoothstep(width * 0.55, width + 0.003, across)) *
                                 step(p.y, 0.0) * smoothstep(0.0, 0.06, uv.y);
                    float heat = saturate(1.0 - radius / 0.235);
                    half3 bodyColor = lerp(half3(0.80, 0.10, 0.015), half3(1.0, 0.40, 0.045), pow(heat, 0.55));
                    float core = 1.0 - smoothstep(0.055, 0.14, radius);
                    bodyColor = lerp(bodyColor, half3(1.0, 0.97, 0.76), core * 0.96);
                    float tailHeat = saturate(1.0 - across / max(width, 0.003)) * pow(1.0 - trailing, 0.55);
                    half3 tailColor = lerp(half3(0.86, 0.09, 0.012), half3(1.0, 0.66, 0.13), tailHeat * 0.8);
                    color *= lerp(tailColor, bodyColor, body);
                    alpha *= max(body, tail * 0.86);
                }
                else if (_Mode < 1.5)
                {
                    float radius = length(uv * 2.0 - 1.0);
                    alpha *= pow(saturate(1.0 - radius), 2.5);
                }
                else if (_Mode < 2.5)
                {
                    float across = abs(uv.y * 2.0 - 1.0);
                    alpha *= pow(saturate(1.0 - across), 1.6);
                }
                else
                {
                    float radius = length(uv * 2.0 - 1.0);
                    alpha *= 1.0 - smoothstep(0.025, 0.09, abs(radius - 0.78));
                }
                clip(alpha - 0.002);
                return half4(saturate(color), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
