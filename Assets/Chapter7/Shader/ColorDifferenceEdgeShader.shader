Shader "Custom/FullScreen/ColorDifferenceEdge"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        float4 _TexelSize;

        float4 Frag(Varyings input) : SV_Target
        {
            // float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);
            // float4 invert = 1 - c;
            // return invert;

            float scale = 2.3;

            float3 center = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord).rgb;
            float3 left   = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord + float2(-scale * _TexelSize.x, 0)).rgb;//tex2D(_IdTex, i.uv + float2(-_PixelSize.x, 0)).rgb;
            float3 right  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord + float2(scale * _TexelSize.x, 0)).rgb;
            float3 up     = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord + float2(0, scale * _TexelSize.y)).rgb;
            float3 down   = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord + float2(0, -scale * _TexelSize.y)).rgb;

            float diff = step(0.1, distance(center, left)) +
                 step(0.1, distance(center, right)) +
                 step(0.1, distance(center, up)) +
                 step(0.1, distance(center, down));

            return diff > 0 ? float4(center, 1) : float4(0, 0, 0, 0);
        }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "NegaPosi"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
