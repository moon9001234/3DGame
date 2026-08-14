Shader "SideScroller3D/Skybox Digital Rain"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0.002, 0.01, 0.004, 1)
        _RainColor ("Rain Color", Color) = (0.05, 1.0, 0.25, 1)
        _HeadColor ("Head Color", Color) = (0.75, 1.0, 0.82, 1)
        _Columns ("Columns", Range(24, 220)) = 92
        _Rows ("Rows", Range(18, 180)) = 74
        _Speed ("Fall Speed", Range(0, 8)) = 2.2
        _Density ("Character Density", Range(0.05, 1)) = 0.68
        _Brightness ("Brightness", Range(0, 6)) = 2.1
        _TrailLength ("Trail Length", Range(0.5, 8)) = 3.5
        _HorizonFade ("Horizon Fade", Range(0, 1)) = 0.25
        _Glitch ("Glitch", Range(0, 1)) = 0.18
        _ScreenScale ("Screen Scale", Range(0.5, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DigitalRainSkybox"
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BackgroundColor;
                half4 _RainColor;
                half4 _HeadColor;
                half _Columns;
                half _Rows;
                half _Speed;
                half _Density;
                half _Brightness;
                half _TrailLength;
                half _HorizonFade;
                half _Glitch;
                half _ScreenScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
            };

            float Hash11(float value)
            {
                return frac(sin(value * 127.1) * 43758.5453);
            }

            float Hash21(float2 value)
            {
                return frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPosition = ComputeScreenPos(output.positionCS);
                return output;
            }

            float RectMask(float2 uv, float2 center, float2 halfSize)
            {
                float2 aa = fwidth(uv) * 1.5 + 0.004;
                float2 mask = smoothstep(halfSize + aa, halfSize - aa, abs(uv - center));
                return mask.x * mask.y;
            }

            float IsDigit(float digit, float target)
            {
                return 1.0 - step(0.5, abs(digit - target));
            }

            float SevenSegmentDigit(float2 glyphUv, float digit)
            {
                glyphUv = saturate((glyphUv - 0.5) * float2(0.82, 0.92) + 0.5);

                float d0 = IsDigit(digit, 0.0);
                float d1 = IsDigit(digit, 1.0);
                float d2 = IsDigit(digit, 2.0);
                float d3 = IsDigit(digit, 3.0);
                float d4 = IsDigit(digit, 4.0);
                float d5 = IsDigit(digit, 5.0);
                float d6 = IsDigit(digit, 6.0);
                float d7 = IsDigit(digit, 7.0);
                float d8 = IsDigit(digit, 8.0);
                float d9 = IsDigit(digit, 9.0);

                float top = max(max(max(d0, d2), max(d3, d5)), max(max(d6, d7), max(d8, d9)));
                float upperRight = max(max(max(d0, d1), max(d2, d3)), max(max(d4, d7), max(d8, d9)));
                float lowerRight = max(max(max(d0, d1), max(d3, d4)), max(max(d5, d6), max(d7, max(d8, d9))));
                float bottom = max(max(max(d0, d2), max(d3, d5)), max(max(d6, d8), d9));
                float lowerLeft = max(max(d0, d2), max(d6, d8));
                float upperLeft = max(max(max(d0, d4), max(d5, d6)), max(d8, d9));
                float middle = max(max(max(d2, d3), max(d4, d5)), max(max(d6, d8), d9));

                float stroke = 0.065;
                float horizontalWidth = 0.27;
                float verticalHeight = 0.19;
                float digitMask = 0.0;
                digitMask = max(digitMask, top * RectMask(glyphUv, float2(0.5, 0.84), float2(horizontalWidth, stroke)));
                digitMask = max(digitMask, middle * RectMask(glyphUv, float2(0.5, 0.5), float2(horizontalWidth, stroke)));
                digitMask = max(digitMask, bottom * RectMask(glyphUv, float2(0.5, 0.16), float2(horizontalWidth, stroke)));
                digitMask = max(digitMask, upperRight * RectMask(glyphUv, float2(0.78, 0.67), float2(stroke, verticalHeight)));
                digitMask = max(digitMask, lowerRight * RectMask(glyphUv, float2(0.78, 0.33), float2(stroke, verticalHeight)));
                digitMask = max(digitMask, lowerLeft * RectMask(glyphUv, float2(0.22, 0.33), float2(stroke, verticalHeight)));
                digitMask = max(digitMask, upperLeft * RectMask(glyphUv, float2(0.22, 0.67), float2(stroke, verticalHeight)));

                float scan = 0.82 + 0.18 * sin((glyphUv.y + digit * 0.17) * 38.0);
                return saturate(digitMask * scan);
            }

            float GlyphPixel(float2 glyphUv, float column, float row, float timeSeed)
            {
                float digit = floor(Hash21(float2(column * 13.7 + timeSeed, row * 5.31)) * 10.0);
                return SevenSegmentDigit(glyphUv, digit);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.screenPosition.xy / max(input.screenPosition.w, 0.0001);
                float aspect = max(0.1, _ScreenParams.x / max(1.0, _ScreenParams.y));
                uv = (uv - 0.5) * float2(aspect, 1.0) * max(0.01, _ScreenScale) + 0.5;
                uv.x = frac(uv.x);

                float columnCount = max(1.0, _Columns);
                float rowCount = max(1.0, _Rows);
                float columnFloat = uv.x * columnCount;
                float column = floor(columnFloat);
                float columnRandom = Hash11(column + 12.37);
                float columnSpeed = _Speed * lerp(0.55, 1.75, columnRandom);
                float rowFloat = uv.y * rowCount + _Time.y * columnSpeed * 8.0;
                float row = floor(rowFloat);
                float2 cellUv = frac(float2(columnFloat, rowFloat));

                float densityMask = step(1.0 - _Density, Hash21(float2(column, row * 0.37)));
                float timeSeed = floor(_Time.y * _Glitch * 16.0);
                float glyph = GlyphPixel(cellUv, column, row, timeSeed);

                float streamPhase = frac(rowFloat / max(1.0, _TrailLength * 9.0) + columnRandom);
                float trail = pow(1.0 - streamPhase, max(0.4, _TrailLength));
                float head = smoothstep(0.92, 1.0, streamPhase);
                float flicker = lerp(0.65, 1.2, Hash21(float2(column * 0.71, row + timeSeed)));
                float verticalFade = smoothstep(0.02, 0.16 + _HorizonFade * 0.32, uv.y);

                float rain = glyph * densityMask * flicker * verticalFade;
                float glow = rain * (0.35 + trail * 1.6 + head * 2.8);
                float columnLine = 1.0 - smoothstep(0.0, 0.48, abs(cellUv.x - 0.5));
                glow += columnLine * densityMask * trail * 0.08;

                half3 color = _BackgroundColor.rgb;
                color += _RainColor.rgb * glow * _Brightness;
                color += _HeadColor.rgb * rain * head * _Brightness;

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
