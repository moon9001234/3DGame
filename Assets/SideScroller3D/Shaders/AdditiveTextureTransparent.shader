Shader "SideScroller3D/Additive Texture Transparent"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Texture", 2D) = "white" {}
        [HDR] [MainColor] _TintColor ("Tint Color", Color) = (0.25, 0.75, 2.0, 1)
        _AdditiveIntensity ("Additive Intensity", Range(0, 10)) = 1
        _Alpha ("Alpha", Range(0, 1)) = 1
        [Toggle] _UseTextureAlpha ("Use Texture Alpha", Float) = 1
        _AlphaPower ("Alpha Power", Range(0.1, 4)) = 1
        [Toggle] _UseVertexColor ("Use Vertex Color", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "AdditiveTransparent"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float fogCoord : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _TintColor;
                half _AdditiveIntensity;
                half _Alpha;
                half _UseTextureAlpha;
                half _AlphaPower;
                half _UseVertexColor;
                half _Cull;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 textureSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 vertexColor = lerp(half4(1, 1, 1, 1), input.color, saturate(_UseVertexColor));

                half textureAlpha = lerp(1.0, textureSample.a, saturate(_UseTextureAlpha));
                half alpha = saturate(textureAlpha * _TintColor.a * _Alpha * vertexColor.a);
                alpha = pow(alpha, _AlphaPower);

                half3 color = textureSample.rgb * _TintColor.rgb * vertexColor.rgb * _AdditiveIntensity;
                color = MixFog(color, input.fogCoord);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
    CustomEditor "AdditiveTextureTransparentShaderGUI"
}
