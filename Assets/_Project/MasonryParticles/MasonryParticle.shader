Shader "JuiceStudy/MasonryParticle"
{
    Properties { _Mode ("Dust 0 / Stone 1 / Brass Spark 2", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
                float _Mode;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                half3 color = input.color.rgb;
                float mask;
                if (_Mode < 0.5)
                {
                    // Soft powder puffs have no hard billboard edge or opaque center.
                    p.x *= 1.03 + 0.06 * p.y;
                    float cloud = saturate(1.0 - dot(p, p));
                    mask = cloud * cloud;
                    color *= 0.94 + 0.06 * p.y;
                }
                else if (_Mode < 1.5)
                {
                    // An irregular clipped stone chip, with a small shaded bevel.
                    float edge = max(abs(p.x) * 0.82 + abs(p.y) * 0.35 - 0.77,
                                     max(-p.y * 0.9 + p.x * 0.1 - 0.8, p.y - 0.76));
                    float feather = max(fwidth(edge), 0.025);
                    mask = 1.0 - smoothstep(-feather, feather, edge);
                    float face = saturate(-edge / 0.16);
                    color *= lerp(0.48, 1.0, face) * (0.91 + 0.14 * p.y - 0.06 * p.x);
                }
                else
                {
                    // Thin velocity-aligned glints with a warm, short-lived core.
                    float core = 1.0 - smoothstep(0.08, 0.42, abs(p.x));
                    float ends = pow(saturate(1.0 - abs(p.y)), 0.65);
                    mask = core * ends;
                    color = lerp(color, half3(1.0, 0.96, 0.82), core * 0.45);
                }
                float alpha = input.color.a * mask;
                clip(alpha - 0.002);
                return half4(saturate(color), alpha);
            }
            ENDHLSL
        }
    }
}
