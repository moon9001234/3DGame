Shader "SideScroller3D/Transparent Glow Cube"
{
    Properties
    {
        [HDR] _BaseColor ("Base Color Bottom", Color) = (0.0, 0.85, 1.0, 0.32)
        [HDR] _BaseColorTop ("Base Color Top", Color) = (1.0, 0.75, 0.1, 0.32)
        [HDR] _EmissionColor ("Emission Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _EdgeColor ("Edge Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _FresnelColor ("Fresnel Color", Color) = (0.3, 0.95, 1.0, 1.0)

        _Alpha ("Transparency", Range(0, 1)) = 0.32
        _SurfaceBrightness ("Surface Brightness", Range(0, 3)) = 1.0
        _EmissionIntensity ("Emission Intensity", Range(0, 20)) = 4.0
        _CoreGlow ("Core Glow", Range(0, 4)) = 0.65

        _BoxExtent ("Object Space Box Extent", Float) = 0.5
        _EdgeWidth ("Edge Width", Range(0.01, 0.45)) = 0.12
        _EdgeIntensity ("Edge Intensity", Range(0, 20)) = 5.0
        _EdgeAlphaBoost ("Edge Alpha Boost", Range(0, 1)) = 0.45

        _FresnelPower ("Fresnel Power", Range(0.25, 8)) = 2.2
        _FresnelIntensity ("Fresnel Intensity", Range(0, 20)) = 4.0
        _FresnelAlphaBoost ("Fresnel Alpha Boost", Range(0, 1)) = 0.35

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
            Name "TransparentCube"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseColorTop;
                float4 _EmissionColor;
                float4 _EdgeColor;
                float4 _FresnelColor;
                float _Alpha;
                float _SurfaceBrightness;
                float _EmissionIntensity;
                float _CoreGlow;
                float _BoxExtent;
                float _EdgeWidth;
                float _EdgeIntensity;
                float _EdgeAlphaBoost;
                float _FresnelPower;
                float _FresnelIntensity;
                float _FresnelAlphaBoost;
                float _Cull;
            CBUFFER_END

            float GetCubeEdgeMask(float3 positionOS)
            {
                float4x4 objectToWorld = GetObjectToWorldMatrix();
                float3 objectScale = float3(
                    length(objectToWorld._m00_m10_m20),
                    length(objectToWorld._m01_m11_m21),
                    length(objectToWorld._m02_m12_m22)
                );
                float3 distanceToFaceWS = max(_BoxExtent - abs(positionOS), 0.0) * max(objectScale, 0.0001);
                float3 edgeAxis = 1.0 - smoothstep(0.0, _EdgeWidth, distanceToFaceWS);
                float edge = edgeAxis.x * edgeAxis.y + edgeAxis.x * edgeAxis.z + edgeAxis.y * edgeAxis.z;
                float corner = edgeAxis.x * edgeAxis.y * edgeAxis.z;
                return saturate(edge + corner);
            }

            float GetVerticalGradient(float3 positionOS)
            {
                return saturate((positionOS.y / max(_BoxExtent * 2.0, 0.0001)) + 0.5);
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.color = IN.color;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceNormalizeViewDir(IN.positionWS));
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                float edge = GetCubeEdgeMask(IN.positionOS);
                float gradient = GetVerticalGradient(IN.positionOS);
                float4 baseGradientColor = lerp(_BaseColor, _BaseColorTop, gradient);

                float alpha = saturate(_Alpha * baseGradientColor.a + edge * _EdgeAlphaBoost + fresnel * _FresnelAlphaBoost);
                float3 vertexTint = IN.color.rgb;
                float3 color = baseGradientColor.rgb * vertexTint * _SurfaceBrightness;

                color += baseGradientColor.rgb * _EmissionColor.rgb * _EmissionIntensity * _CoreGlow;
                color += _EdgeColor.rgb * _EdgeIntensity * edge;
                color += _FresnelColor.rgb * _FresnelIntensity * fresnel;

                return half4(color, alpha);
            }
            ENDHLSL
        }

    }

    FallBack Off
}
