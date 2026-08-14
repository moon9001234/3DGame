Shader "SideScroller3D/Toon Texture Vertical Gradient"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        _ShadowColor ("Shadow Color", Color) = (0.45, 0.48, 0.56, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.3)) = 0.04
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.28

        _GradientStartColor ("Gradient Start Color", Color) = (0.05, 0.2, 0.85, 1)
        _GradientEndColor ("Gradient End Color", Color) = (0.85, 1, 1, 1)
        [HideInInspector] _GradientUseXAxis ("Use X Axis", Float) = 0
        [HideInInspector] _GradientUseYAxis ("Use Y Axis", Float) = 0
        [HideInInspector] _GradientUseZAxis ("Use Z Axis", Float) = 1
        _GradientCenter ("Gradient Center", Float) = 0
        _GradientWidth ("Gradient Width", Float) = 2
        _GradientStrength ("Gradient Strength", Range(0, 1)) = 0.45
        _GradientToonBlend ("Gradient Toon Blend", Range(0, 1)) = 1
        [Toggle] _UseWorldGradient ("Use World Gradient", Float) = 0

        [HideInInspector] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull[_Cull]
            ZWrite On
            ZTest LEqual

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
                float3 positionOS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float fogCoord : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half _ShadowStrength;
                half _AmbientStrength;
                half4 _GradientStartColor;
                half4 _GradientEndColor;
                half _GradientUseXAxis;
                half _GradientUseYAxis;
                half _GradientUseZAxis;
                half _GradientCenter;
                half _GradientWidth;
                half _GradientStrength;
                half _GradientToonBlend;
                half _UseWorldGradient;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.positionOS = input.positionOS.xyz;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);

                Light mainLight = GetMainLight(input.shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half lightAmount = ndotl * mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                half toonLight = smoothstep(
                    _ShadowThreshold - _ShadowSmoothness,
                    _ShadowThreshold + _ShadowSmoothness,
                    lightAmount);

                half3 bakedLight = SampleSH(normalWS) * _AmbientStrength;
                half3 litColor = baseSample.rgb * (mainLight.color * max(toonLight, 0.001) + bakedLight);
                half3 shadowColor = baseSample.rgb * _ShadowColor.rgb;
                half shadowMask = (1.0 - toonLight) * _ShadowStrength;
                half3 toonColor = lerp(litColor, shadowColor + bakedLight * baseSample.rgb, shadowMask);

                half3 gradientAxis = half3(
                    step(0.5, _GradientUseXAxis),
                    step(0.5, _GradientUseYAxis),
                    step(0.5, _GradientUseZAxis));
                gradientAxis = any(gradientAxis > 0.5) ? normalize(gradientAxis) : half3(0, 0, 1);
                half3 gradientPosition = lerp(input.positionOS, input.positionWS, saturate(_UseWorldGradient));
                half gradientProjection = dot(gradientPosition, gradientAxis);
                half gradientT = saturate((gradientProjection - _GradientCenter) / max(abs(_GradientWidth), 0.0001) + 0.5);
                half4 gradientColor = lerp(_GradientStartColor, _GradientEndColor, gradientT);
                half gradientMask = gradientColor.a * _GradientStrength;
                half3 gradientOverToon = lerp(toonColor * gradientColor.rgb, gradientColor.rgb, _GradientToonBlend);
                half3 finalColor = lerp(toonColor, gradientOverToon, gradientMask);

                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, baseSample.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull[_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

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
    CustomEditor "ToonTextureVerticalGradientShaderGUI"
}
