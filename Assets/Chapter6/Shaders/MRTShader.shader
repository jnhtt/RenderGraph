Shader "Custom/MRTShader"
{
    Properties
    { }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };
            struct Output
            {
                float4 colorR : COLOR0;
                float4 colorG : COLOR1;
                float4 colorB : COLOR2;
            };
            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            Output frag(Varyings i)
            {
                Output o = (Output)0;
                o.colorR = float4(1, 0, 0, 1);
                o.colorG = float4(0, 1, 0, 1);
                o.colorB = float4(0, 0, 1, 1);
                return o;
            }
            ENDHLSL
        }
    }
}
