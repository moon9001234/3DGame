Shader "SideScroller3D/Toon Dot Shadow"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.45, 0.48, 0.55, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.48
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.25)) = 0.06
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        [HideInInspector] _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _Cull ("Cull", Float) = 2

        _DotMap ("Shadow Dot Texture", 2D) = "white" {}
        _DotTiling ("Dot Tiling", Float) = 36
        _DotPixelSize ("Dot Pixel Size", Float) = 10
        _DotMinRadius ("Dot Min Radius", Range(0, 0.5)) = 0
        _DotMaxRadius ("Dot Max Radius", Range(0.05, 0.75)) = 0.48
        _DotEdgeWidth ("Dot Edge Width", Range(0.01, 0.8)) = 0.36
        _DotSoftness ("Dot Edge Softness", Range(0.001, 0.15)) = 0.025
        _DotTextureBlend ("Dot Texture Blend", Range(0, 1)) = 0
        _DotStrength ("Dot Strength", Range(0, 1)) = 1
        _DotContrast ("Dot Texture Contrast", Range(0.1, 4)) = 1.4
        [Toggle] _InvertDot ("Invert Dot Texture", Float) = 0

        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.28
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DotMap);
            SAMPLER(sampler_DotMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float _ShadowThreshold;
                float _ShadowSmoothness;
                float _ShadowStrength;
                float _DotTiling;
                float _DotPixelSize;
                float _DotMinRadius;
                float _DotMaxRadius;
                float _DotEdgeWidth;
                float _DotSoftness;
                float _DotTextureBlend;
                float _DotStrength;
                float _DotContrast;
                float _InvertDot;
                float _AmbientStrength;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normalWS = NormalizeNormalPerPixel(input.normalWS);

                Light mainLight = GetMainLight(input.shadowCoord);
                float ndotl = saturate(dot(normalWS, mainLight.direction));
                float lightAmount = ndotl * mainLight.shadowAttenuation * mainLight.distanceAttenuation;

                float toonLit = smoothstep(
                    _ShadowThreshold - _ShadowSmoothness,
                    _ShadowThreshold + _ShadowSmoothness,
                    lightAmount);

                float halfDotBand = max(_DotEdgeWidth * 0.5, 0.001);
                float shadowCoreEnd = _ShadowThreshold - halfDotBand;
                float dotBandEnd = _ShadowThreshold + halfDotBand;
                float solidShadow = 1.0 - smoothstep(shadowCoreEnd, shadowCoreEnd + _DotSoftness, lightAmount);
                float edgeAmount = saturate((dotBandEnd - lightAmount) / max(_DotEdgeWidth, 0.001));
                float edgeBand = smoothstep(shadowCoreEnd, shadowCoreEnd + _DotSoftness, lightAmount)
                    * (1.0 - smoothstep(dotBandEnd - _DotSoftness, dotBandEnd, lightAmount));

                float2 halftoneUV = input.positionWS.xy * max(_DotTiling, 0.001) / max(_DotPixelSize, 1.0);
                float2 cellUV = frac(halftoneUV) - 0.5;
                float dotRadius = lerp(_DotMinRadius, _DotMaxRadius, edgeAmount);
                float proceduralDot = 1.0 - smoothstep(dotRadius, dotRadius + _DotSoftness, length(cellUV));

                float textureDot = SAMPLE_TEXTURE2D(_DotMap, sampler_DotMap, halftoneUV).r;
                textureDot = saturate((textureDot - 0.5) * _DotContrast + 0.5);
                textureDot = lerp(textureDot, 1.0 - textureDot, step(0.5, _InvertDot));

                float dotMask = lerp(proceduralDot, textureDot, _DotTextureBlend);
                float dottedEdge = dotMask * edgeBand * _DotStrength;
                float dottedShadow = saturate(max(solidShadow, dottedEdge) * _ShadowStrength);

                float3 litColor = baseSample.rgb * mainLight.color;
                float3 ambientColor = baseSample.rgb * _AmbientStrength;
                float3 shadowColor = baseSample.rgb * _ShadowColor.rgb;
                float3 finalColor = lerp(litColor + ambientColor, shadowColor + ambientColor, dottedShadow);

                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, baseSample.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(output.positionCS);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
