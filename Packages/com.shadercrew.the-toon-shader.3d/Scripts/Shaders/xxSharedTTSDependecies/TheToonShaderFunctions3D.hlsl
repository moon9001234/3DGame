#ifndef THETOONSHADER_FUNCTION
#define THETOONSHADER_FUNCTION








        

































struct GeneralStylingData
{
    half enableDistanceFade;
    float distanceFadeStartDistance;
    float distanceFadeFalloff;
    half adjustDistanceFadeValue;
    float distanceFadeValue;
};


struct StylingData
{
    half isEnabled;
    half style;
    half type;
    float4 color;
    float rotation;
    float rotationBetweenCells;
    float density;
    float offset;
    float size;
    float sizeMin;
    half sizeMinFromControlMap;
    float sizeControl;
    float sizeFalloff;
    float roundness;
    float roundnessFalloff;
    float hardness;
    float opacity;
    float opacityFalloff;


    float dashEnabled;
    float dashType;
    float dashLength;
    float dashDensity;
    float dashTransitionPosition;
    float dashTransitionSoftness;
    float dashRoundness;
    float dashOffset;
};

struct StylingRandomData
{
    float enableRandomizer;
    float perlinNoiseSize;
    float perlinNoiseSeed;
    float whiteNoiseSeed;
    
    float noiseIntensity;
    
    half spacingRandomMode;
    float spacingRandomIntensity;

    half opacityRandomMode; 
    float opacityRandomIntensity;

    half lengthRandomMode;
    float lengthRandomIntensity;

    half hardnessRandomMode;
    float hardnessRandomIntensity;

    half thicknessRandomMode; 
    float thicknesshRandomIntensity;
    
   
   

};

struct AdditionalStylingSpecularData
{
    
};

struct AdditionalStylingRimData
{
    
};

struct PositionAndBlendingData
{
    half position;
    half blending;
    half isInverted;
};

struct UVSets
{
    float2 uv0;
    float2 uv1;
    float2 uv2;
    float2 uv3;
};

struct UVSpaceData
{
    half drawSpace;
    half uvSet;
    half coordinateSystem;
    half polarCenterMode;
    float4 polarCenter;
    half sSCameraDistanceScaled;
    half anchorSSToObjectsOrigin;
};


struct NoiseSampleData
{
    float perlinNoise;
    
    
    float perlinNoiseFloored;
    float whiteNoise;
    float whiteNoiseFloored;
};

struct RequiredNoiseData
{
    bool perlinNoise;
    bool perlinNoiseFloored;
    bool whiteNoise;
    bool whiteNoiseFloored;
};


#define UNITY_TWO_PI        6.28318530718f

float shiftLinear(
float ll0, float lll0
)
{
    float llll0 = (ll0 - lll0) / max(lll0 + 1.0, 1e-6);
    float lllll0 = (ll0 - lll0) / max(1.0 - lll0, 1e-6);
    return lerp(llll0, lllll0, step(lll0, ll0)); 
}
float sum(
float3 lllllll0
)
{
    return dot(lllllll0, float3(1, 1, 1));
}
float invLerp(
float lllllllll0, float llllllllll0, float ll0
)
{
    return (ll0 - lllllllll0) / (llllllllll0 - lllllllll0);
}
float4 invLerp(
float4 lllllllll0, float4 llllllllll0, float4 ll0
)
{
    return (ll0 - lllllllll0) / (llllllllll0 - lllllllll0);
}
float remap(
float lllllllllllllllll0, float llllllllllllllllll0, float lllllllllllllllllll0, float llllllllllllllllllll0, float ll0
)
{
    float llllllllllllllllllllll0 = invLerp(lllllllllllllllll0, llllllllllllllllll0, ll0);
    return lerp(lllllllllllllllllll0, llllllllllllllllllll0, llllllllllllllllllllll0);
}
float2 GetScreenUV(
float2 llllllllllllllllllllllll0, float lllllllllllllllllllllllll0
)
{
#if _URP
    float4 llllllllllllllllllllllllll0 = TransformObjectToHClip(float3(0, 0, 0));
#else
    float4 llllllllllllllllllllllllll0 = UnityObjectToClipPos(float3(0, 0, 0));
#endif
    float2 llllllllllllllllllllllllllll0 = float2(llllllllllllllllllllllll0.x, llllllllllllllllllllllll0.y);
    float lllllllllllllllllllllllllllll0 = _ScreenParams.y / _ScreenParams.x;
    llllllllllllllllllllllllllll0.x -= llllllllllllllllllllllllll0.x / (llllllllllllllllllllllllll0.w);
    llllllllllllllllllllllllllll0.y -= llllllllllllllllllllllllll0.y / (llllllllllllllllllllllllll0.w);
    llllllllllllllllllllllllllll0.y *= lllllllllllllllllllllllllllll0;
    llllllllllllllllllllllllllll0 *= 1 / lllllllllllllllllllllllll0;
    llllllllllllllllllllllllllll0 *= llllllllllllllllllllllllll0.z;
    return llllllllllllllllllllllllllll0;
};
float2 toPolar(
float2 lllllllllllllllllllllllllllllll0
)
{
    float l1 = length(lllllllllllllllllllllllllllllll0);
    float ll1 = atan2(lllllllllllllllllllllllllllllll0.y, lllllllllllllllllllllllllllllll0.x);
    return float2(ll1 / UNITY_TWO_PI, l1);
}
float2 ConvertToDrawSpace(
#if _URP
    InputData inputData, 
#else
    float3 llll1,
    float3 lllll1,
#endif
float2 lllllllll1, UVSpaceData uvSpaceData, float4 llllllllllllllllllllllllllll0, UVSets uvSets
)
{
#if _URP
        float3 llll1 = inputData.positionWS;
        float3 lllll1 = inputData.normalWS;
#endif      
    if (uvSpaceData.drawSpace == 0)    
    {
        if (uvSpaceData.uvSet == 0.0)
        {
            lllllllll1 = uvSets.uv0;
        }
        else if (uvSpaceData.uvSet == 1.0)
        {
            lllllllll1 = uvSets.uv1;
        }
        else if (uvSpaceData.uvSet == 2.0)
        {
            lllllllll1 = uvSets.uv2;
        }
        else if (uvSpaceData.uvSet == 3.0)
        {
            lllllllll1 = uvSets.uv3;
        }
    }
    else if (uvSpaceData.drawSpace == 1)    
    {
        float4 llllllllllllllllllllllll0 = mul(UNITY_MATRIX_VP, float4(llll1, 1.0));
        float4 llllllllllllll1 = ComputeScreenPos(llllllllllllllllllllllll0);
        lllllllll1 = ((llllllllllllll1.xy) / llllllllllllll1.w); 
        if (uvSpaceData.anchorSSToObjectsOrigin)
        {
            float4 lllllllllllllll1 = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos, 1.0));
            float2 llllllllllllllll1 = lllllllllllllll1.xy / lllllllllllllll1.w;
            float2 lllllllllllllllll1 = llllllllllllllllllllllllllll0.xy;
            lllllllll1 = lllllllll1 - lllllllllllllllll1; 
        }
    }
    else if (uvSpaceData.drawSpace == 2)    
    {
        float3 lllllllllllllllllll1 = abs(lllll1);
        if (lllllllllllllllllll1.x > lllllllllllllllllll1.y && lllllllllllllllllll1.x > lllllllllllllllllll1.z)
        {
            lllllllll1 = llll1.yz;
        }
        else if (lllllllllllllllllll1.y > lllllllllllllllllll1.z)
        {
            lllllllll1 = llll1.xz;
        }
        else
        {
            lllllllll1 = llll1.xy;
        }
    }
    if (uvSpaceData.coordinateSystem == 1) 
    {
        if (uvSpaceData.drawSpace == 1)
        {
            if (uvSpaceData.polarCenterMode == 0) 
            {
                lllllllll1.xy -= uvSpaceData.polarCenter.xy;
            }
            else 
            {
                uvSpaceData.polarCenter.a = 1;
                float4 llllllllllllllllllll1 = mul(UNITY_MATRIX_VP, uvSpaceData.polarCenter);
                float4 lllllllllllllllllllll1 = ComputeScreenPos(llllllllllllllllllll1);
                float2 llllllllllllllllllllll1 = lllllllllllllllllllll1.xy / lllllllllllllllllllll1.w;
                lllllllll1.xy -= llllllllllllllllllllll1;
            }
        }
        else
        {
            lllllllll1.xy -= uvSpaceData.polarCenter.xy;
        }
    }
    if (uvSpaceData.coordinateSystem == 1) 
    {
        lllllllll1 = toPolar(lllllllll1);
    }
    if (uvSpaceData.drawSpace == 1)
    {
        if (uvSpaceData.sSCameraDistanceScaled == 1)
        {
            float3 lllllllllllllllllllllll1 = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1.0)).xyz;
            lllllllll1.xy *= distance(_WorldSpaceCameraPos, lllllllllllllllllllllll1);
        }
        float llllllllllllllllllllllll1 = _ScreenParams.x / _ScreenParams.y;
        lllllllll1.x *= llllllllllllllllllllllll1;
    }
    return lllllllll1;
}
float2 PixelateDrawSpaceUV(
float2 lllllllll1, UVSpaceData uvSpaceData, half lllllllllllllllllllllllllll1, float llllllllllllllllllllllllllll1
)
{
    if (lllllllllllllllllllllllllll1 != 1 || llllllllllllllllllllllllllll1 <= 0.0)
    {
        return lllllllll1;
    }
    float2 lllllllllllllllllllllllllllll1;
    if (uvSpaceData.drawSpace == 1)
    {
        float llllllllllllllllllllllllllllll1 = max(llllllllllllllllllllllllllll1, 1.0) / max(_ScreenParams.y, 1.0);
        if (uvSpaceData.sSCameraDistanceScaled == 1)
        {
            float3 lllllllllllllllllllllll1 = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1.0)).xyz;
            llllllllllllllllllllllllllllll1 *= max(distance(_WorldSpaceCameraPos, lllllllllllllllllllllll1), 0.0001);
        }
        lllllllllllllllllllllllllllll1 = float2(llllllllllllllllllllllllllllll1, llllllllllllllllllllllllllllll1);
    }
    else
    {
        lllllllllllllllllllllllllllll1 = max(llllllllllllllllllllllllllll1, 0.0001) / 4096.0;
    }
    return (floor(lllllllll1 / lllllllllllllllllllllllllllll1) + 0.5) * lllllllllllllllllllllllllllll1;
}
float CalculateSpecularMaskSkipDot(
float ll2, float3 lll2, float llll2, float lllll2, float llllll2
)
{
    float lllllll2 = 0;
    float llllllll2 = (1 - (llll2)) * 10; 
    ll2 = max(ll2, 0); 
    float lllllllll2 = pow(ll2, llllllll2 * llllllll2);
    float llllllllll2 = smoothstep(0.8, 0.8 + lllll2 / 1, lllllllll2);
    lllllll2 = llllllllll2 * llllll2 * 5;
    return lllllll2;
}
float CalculateSpecularMask(
float3 llllllllllll2, float3 lllllllllllll2, float3 lll2, float llll2, float lllll2, float llllll2
)
{
    float lllllll2 = 0;
    float3 lllllllllllllllllll2 = normalize(lllllllllllll2 + lll2);
    float ll2 = dot(llllllllllll2, lllllllllllllllllll2);
    lllllll2 = CalculateSpecularMaskSkipDot(ll2, lll2, llll2, lllll2, llllll2);
    return lllllll2;
}
float CalculateRimMask(
float3 lll4, float3 lll2, float lllll4, float llllll4, float llllll2,
                        half llllllll4, half lllllllll4, half llllllllll4, float lllllllllll4
)
{
    float llllllllllll4 = 0;
    float lllllllllllll4 = saturate(1 - dot(lll2, lll4));
    lllll4 = 1 - lllll4;
    float llllllllllllll4 = smoothstep(saturate(lllll4 - llllll4), lllll4, lllllllllllll4);
    if ((llllllll4 == 0 && llllll2 > 0.0 && ((lllllllllll4 >= 0 || lllllllll4 == 0) || llllllllll4 == 0))
    || (llllllll4 == 1 && (llllll2 <= 0.0 || (lllllllllll4 <= 2 && lllllllll4 == 1)))
    || llllllll4 == 2)
    {
        if (llllllll4 == 1)
        {
            float lllllllllllllll4 = llllll2;
            if (lllllllll4)
            {
                if (llllll2 > 0)
                {
                    llllll2 *= lllllllllll4;
                }
            }
            {
                float llllllllllllllll4 = 1 - abs(min(llllll2 * 2, 0)); 
                if (lllllllllllllll4 > 0)
                {
                    llllllllllllllll4 = lllllllllll4;
                }
                llllllllllll4 = llllllllllllll4 * (1 - llllllllllllllll4);
            }
        }
        else if (llllllll4 == 0)
        {
            llllllllllll4 = llllllllllllll4 * (llllll2 * 2) * (lllllllllll4);
        }
        else if (llllllll4 == 2)
        {
            llllllllllll4 = llllllllllllll4; 
        }
    }
    return llllllllllll4;
}
float CalculateRimMask2(
float3 lll4, float3 lll2, float lllll4, float llllll4, float llllll2,
                        half llllllll4, half lllllllll4, half llllllllll4, float lllllllllll4
)
{
    float llllllllllll4 = 0;
    float lllllllllllll4 = saturate(1 - dot(lll2, lll4));
    lllll4 = 1 - lllll4;
    float llllllllllllll4 = smoothstep(saturate(lllll4 - llllll4), lllll4, lllllllllllll4);
    if ((llllllll4 == 0 && llllll2 > 0.0 && ((lllllllllll4 >= 0 || lllllllll4 == 0) || llllllllll4 == 0))
    || (llllllll4 == 1 && (llllll2 <= 0.0 || (lllllllllll4 <= 2 && lllllllll4 == 1)))
    || llllllll4 == 2)
    {
        if (llllllll4 == 1)
        {
            if (lllllllll4)
            {
                llllllllllll4 = llllllllllllll4 * (1 - lllllllllll4);
            }
            else
            {
                float llllllllllllllll4 = 1 - abs(min(llllll2 * 2, 0)); 
                float lllllll0 = lerp(0, llllllllllllllll4 * 4, llllll4);
                llllllllllll4 = llllllllllllll4 * (1 - llllllllllllllll4);
            }
        }
        else if (llllllll4 == 2)
        {
            llllllllllll4 = llllllllllllll4; 
        }
        else
        {
            llllllllllll4 = llllllllllllll4 * (llllll2 * 2) * (lllllllllll4);
        }
    }
    return llllllllllll4;
}
float2 RotateUV(
float2 lllllllll1, float ll1
)
{
    float llll5 = radians(ll1);
    float lllll5 = cos(llll5);
    float llllll5 = sin(llll5);
    float2 lllllll5;
    lllllll5.x = lllllllll1.x * lllll5 - lllllllll1.y * llllll5;
    lllllll5.y = lllllllll1.x * llllll5 + lllllllll1.y * lllll5;
    return lllllll5;
}
float2 RotateUVRadians(
float2 lllllllll1, float llllllllll5
)
{
    float llll5 = llllllllll5;
    float lllll5 = cos(llll5);
    float llllll5 = sin(llll5);
    float2 lllllll5;
    lllllll5.x = lllllllll1.x * lllll5 - lllllllll1.y * llllll5;
    lllllll5.y = lllllllll1.x * llllll5 + lllllllll1.y * lllll5;
    return lllllll5;
}
float CalculateHatchingDashContinuity(
float llllllllllllllll5, float lllllllllllllllll5, float llllllllllllllllll5
)
{
    float lllllllllllllllllll5 = saturate(llllllllllllllllll5);
    float llllllllllllllllllll5 = saturate(llllllllllllllll5);
    float lllllllllllllllllllll5 = max(lllllllllllllllll5, 0.0001);
    float llllllllllllllllllllll5 = saturate(llllllllllllllllllll5 + lllllllllllllllllllll5);
    float lllllllllllllllllllllll5 = llllllllllllllllllllll5 - llllllllllllllllllll5;
    float llllllllllllllllllllllll5 = 0.0;
    if (lllllllllllllllllllllll5 <= 0.0001)
    {
        llllllllllllllllllllllll5 = step(llllllllllllllllllll5, lllllllllllllllllll5);
    }
    else
    {
        llllllllllllllllllllllll5 = smoothstep(llllllllllllllllllll5, llllllllllllllllllllll5, lllllllllllllllllll5);
    }
    llllllllllllllllllllllll5 *= llllllllllllllllllllllll5;
    return 1.0 - llllllllllllllllllllllll5;
}
float CalculateHatchingDashSafeSpacingHalfWidth(
float llllllllllllllllllllllllll5, float lllllllllllllllllllllllllll5, float llllllllllllllllllllllllllll5, half lllllllllllllllllllllllllllll5
)
{
    float llllllllllllllllllllllllllllll5 = max(llllllllllllllllllllllllll5, 0.0);
    float lllllllllllllllllllllllllllllll5 = saturate(lllllllllllllllllllllllllll5);
    float l6 = llllllllllllllllllllllllll5 * (1.0 - lllllllllllllllllllllllllllllll5);
    float ll6 = lllllllllllllllllllllllllllll5 ? max(llllllllllllllllllllllllllll5, 0.0) : 0.0;
    float lll6 = 0.002;
    llllllllllllllllllllllllllllll5 += max(l6, ll6) + lll6;
    return min(llllllllllllllllllllllllllllll5, 0.499);
}
float CalculateHatching1DMaskFromDistance(
float lllll6, float llllll6, float lllllllllllllllllllllllllll5, float llllllll6, float lllllllll6, float llllllllll6, half lllllllllllllllllllllllllllll5
)
{
    if (llllll6 <= 0.0)
    {
        return 0.0;
    }
    float lllllllllllllllllllllllllllllll5 = saturate(lllllllllllllllllllllllllll5);
    float lllllllllllll6 = 1.0 - step(llllll6, lllll6);
    float llllllllllllll6 = llllll6 * (1.0 - lllllllllllllllllllllllllllllll5);
    float lllllllllllllll6 = lllllllllllllllllllllllllllll5 ? max(llllllllll6, 0.0) : 0.0;
    if (abs(llllll6 - llllllll6) < 0.00001 && abs(lllllllll6 - 1.0) < 0.00001)
    {
        lllllllllllllll6 = 0.0;
    }
    if (!lllllllllllllllllllllllllllll5 || lllllllllllllll6 <= 0.0)
    {
        if (llllllllllllll6 <= 0.000001)
        {
            return lllllllllllll6;
        }
        return 1.0 - smoothstep(llllll6 - llllllllllllll6, llllll6, lllll6);
    }
    float llllllllllllllll6 = max(llllllllllllll6, min(lllllllllllllll6, llllll6));
    if (llllllllllllllll6 <= 0.000001)
    {
        return lllllllllllll6;
    }
    return 1.0 - smoothstep(llllll6 - llllllllllllllll6, llllll6, lllll6);
}
float ApplyHatchingDashMode(
float llllllllllllllllll6, float lllllllllllllllllll6, float llllllllllllllllllll6, float lllllllllllllllllllll6, float llllllllllllllllllllll6, float lllllllllllllllllllllllllll5,
float llllllllllllllllllllllllllll5, float lllllllllllllllllllllllll6, float llllllllllllllllllllllllll6, float lllllllllllllllllllllllllll6, half lllllllllllllllllllllllllllll5
)
{
    if (lllllllllllllllllllll6 <= 0.0)
    {
        return llllllllllllllllll6;
    }
    float lllllllllllllllllllllllllllll6 = max(0.0, 0.5 - llllllllllllllllllllll6);
    float llllllllllllllllllllllllllllll6 = 0.0;
    if (lllllllllllllllllllllllllllll6 > 0.00001)
    {
        llllllllllllllllllllllllllllll6 = smoothstep(0.0, 0.001, lllllllllllllllllllllllllllll6);
    }
    if (llllllllllllllllllllllllll6 < 0.5)
    {
        if (llllllllllllllllllllllllllllll6 <= 0.0)
        {
            return llllllllllllllllll6;
        }
        float lllllllllllllllllllllllllllllll6 = CalculateHatching1DMaskFromDistance(
            llllllllllllllllllll6,
            llllllllllllllllllllll6,
            lllllllllllllllllllllllllll5,
            -1.0,
            0.0,
            lllllllllllllllllllllllll6,
            lllllllllllllllllllllllllllll5
        );
        float l7 = llllllllllllllllll6 * lllllllllllllllllllllllllllllll6;
        return lerp(llllllllllllllllll6, l7, llllllllllllllllllllllllllllll6);
    }
    float ll7 = remap(
        0.0, 1.0,
        max(min(lllllllllllllllllllll6, max(llllllllllllllllllllll6, 0.0001)), 0.0001),
        0.0001,
        saturate(lllllllllllllllllllllllllll5)
    );
    float lll7 = lllllllllllllllllllllllllllll5 ? max(lllllllllllllllllllllllll6, 0.0) : 0.0;
    ll7 = max(ll7, lll7 + 0.0001);
    float llll7 = 0.5 + ll7;
    float lllll7 = lerp(llll7, llllllllllllllllllllll6, llllllllllllllllllllllllllllll6);
    float llllll7 = lllllllllllllllllllllllllll6 * llllllllllllllllllllllllllllll6;
    float2 lllllll7 = float2(llllllllllllllllllll6, lllllllllllllllllll6);
    float2 llllll6 = float2(max(lllll7, 0.0001), max(lllllllllllllllllllll6, 0.0001));
    float lllllllllllllllllllllllllllllll5 = saturate(lllllllllllllllllllllllllll5);
    float llllllllll7 = max(lllllllllllllllllllllllllllll5 ? max(max(llllllllllllllllllllllllllll5, lllllllllllllllllllllllll6), 0.0) : 0.0, 0.0001);
    float lllllllllll7 = max(min(llllll6.x, llllll6.y), 0.0001);
    float llllllllllll7 = max((0.5 - llllllllll7) / max(2.0 - lllllllllllllllllllllllllllllll5, 1.0), 0.0001);
    float lllllllllllll7 = lerp(
        lllllllllll7,
        min(lllllllllll7, llllllllllll7),
        llllllllllllllllllllllllllllll6
    );
    float llllllllllllll7 = max(0.5 - (lllllllllllll7 * (1.0 - lllllllllllllllllllllllllllllll5)) - llllllllll7, 0.0001);
    llllll6 = lerp(llllll6, min(llllll6, float2(llllllllllllll7, llllllllllllll7)), llllllllllllllllllllllllllllll6);
    float lllllllllllllll7 = max(min(llllll6.x, llllll6.y), 0.0001);
    float llllllllllllllllllllllllllll3 = lllllllllllllll7 * saturate(llllll7);
    llllllllllllllllllllllllllll3 = min(llllllllllllllllllllllllllll3, min(llllll6.x, llllll6.y));
    float2 lllllllllllllllll7 = abs(lllllll7) - (llllll6 - llllllllllllllllllllllllllll3);
    float llllllllllllllllll7 = length(max(lllllllllllllllll7, 0.0)) + min(max(lllllllllllllllll7.x, lllllllllllllllll7.y), 0.0) - llllllllllllllllllllllllllll3;
    float lllllllllllll6 = 1.0 - step(0.0, llllllllllllllllll7);
    float llllllllllllll6 = remap(
        0.0, 1.0,
        lllllllllllllll7,
        0.0,
        lllllllllllllllllllllllllllllll5
    );
    if (!lllllllllllllllllllllllllllll5)
    {
        if (llllllllllllll6 <= 0.000001)
        {
            return lllllllllllll6;
        }
        return 1.0 - smoothstep(0.0, llllllllllllll6, llllllllllllllllll7);
    }
    float2 lllllllllllllllllllll7 = float2(max(lllllllllllllllllllllllll6, 0.0), max(llllllllllllllllllllllllllll5, 0.0));
    float2 llllllllllllllllllllll7;
    if (lllllllllllllllll7.x > 0.0 && lllllllllllllllll7.y > 0.0)
    {
        float2 lllllllllllllllllllllll7 = max(lllllllllllllllll7, 0.0);
        llllllllllllllllllllll7 = lllllllllllllllllllllll7 / max(length(lllllllllllllllllllllll7), 0.0001);
    }
    else
    {
        llllllllllllllllllllll7 = (lllllllllllllllll7.x > lllllllllllllllll7.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    }
    float lllllllllllllll6 = min(dot(llllllllllllllllllllll7, lllllllllllllllllllll7), lllllllllllllll7);
    if (lllllllllllllll6 <= 0.000001)
    {
        if (llllllllllllll6 <= 0.000001)
        {
            return lllllllllllll6;
        }
        return 1.0 - smoothstep(0.0, llllllllllllll6, llllllllllllllllll7);
    }
    if (llllllllllllll6 >= lllllllllllllll6)
    {
        return 1.0 - smoothstep(0.0, llllllllllllll6, llllllllllllllllll7);
    }
    float lllllllllllllllllllllllll7 = saturate(llllllllllllll6 / lllllllllllllll6);
    float llllllllllllllllllllllllll7 = lerp(-lllllllllllllll6, 0.0, lllllllllllllllllllllllll7);
    float lllllllllllllllllllllllllll7 = lllllllllllllll6;
    return 1.0 - smoothstep(llllllllllllllllllllllllll7, lllllllllllllllllllllllllll7, llllllllllllllllll7);
}
NoiseSampleData SampleNoiseData(
float2 lllllllll1, StylingData stylingData, StylingRandomData stylingRandomData, RequiredNoiseData requiredNoiseData,
#ifdef USE_UNITY_TEXTURE_2D_TYPE
    UnityTexture2D lllllllllllllllllllllllllllll7, UnityTexture2D llllllllllllllllllllllllllllll7
#else
    sampler2D lllllllllllllllllllllllllllll7, sampler2D llllllllllllllllllllllllllllll7
#endif
)
{
    NoiseSampleData noiseSampleData;
    if (stylingRandomData.enableRandomizer == 1)
    {
        if (stylingData.style == 1)
        {
            if (fmod(floor(lllllllll1.y * stylingData.density), 2) == 0)
            {
                lllllllll1.x += stylingData.offset / stylingData.density;
            }
        }
        float lllllllllllllllllllllllllllllll7 = 0;
        if (requiredNoiseData.perlinNoiseFloored == 1)
        {
            float2 l8 = lllllllll1;
            l8.x = floor(lllllllll1.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 1)
            {
                l8.y = floor(lllllllll1.y * stylingData.density) / stylingData.density;
            }
            l8 *= stylingRandomData.perlinNoiseSize;
            lllllllllllllllllllllllllllllll7 = tex2Dlod(lllllllllllllllllllllllllllll7, float4(l8, 0.0, 0.0)).x;
        }
        float ll8 = 0;
        if (requiredNoiseData.perlinNoise == 1)
        {
            float2 lll8 = lllllllll1 * stylingRandomData.perlinNoiseSize;
            ll8 = tex2Dlod(lllllllllllllllllllllllllllll7, float4(lll8, 0.0, 0.0)).x; 
        }
        float llll8 = 0;
        if (requiredNoiseData.whiteNoise == 1)
        {
            float2 lllll8 = lllllllll1;
            lllll8.x = floor(lllllllll1.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 0)
            {
                lllll8.y = 0.1;
            }
            else if (stylingData.style == 1)
            {
                lllll8.y = floor(lllllllll1.y * stylingData.density) / stylingData.density;
            }
            llll8 = tex2Dlod(llllllllllllllllllllllllllllll7, float4(lllll8, 0.0, 0.0)).x; 
        }
        float llllll8 = 0;
        if (requiredNoiseData.whiteNoiseFloored == 1)
        {
            float2 lllllll8 = lllllllll1;
            lllllll8.x = floor(lllllllll1.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 0)
            {
                lllllll8.y = 0.1;
            }
            else if (stylingData.style == 1)
            {
                lllllll8.y = 0.1;
            }
            llllll8 = tex2Dlod(llllllllllllllllllllllllllllll7, float4(lllllll8, 0.0, 0.0)).x; 
        }
        noiseSampleData.perlinNoise = ll8;
        noiseSampleData.perlinNoiseFloored = lllllllllllllllllllllllllllllll7;
        noiseSampleData.whiteNoise = llll8;
        noiseSampleData.whiteNoiseFloored = llllll8;
    }
    else
    {
        noiseSampleData.perlinNoise = 0;
        noiseSampleData.perlinNoiseFloored = 0;
        noiseSampleData.whiteNoise = 0;
        noiseSampleData.whiteNoiseFloored = 0;
    }
    return noiseSampleData;
}
float Hatching(
float ll0, float2 lllllllll1, StylingData hatchingData, StylingRandomData stylingRandomData, NoiseSampleData noiseSampleData, half lllllllllll8
)
{
    ll0 = 1 - ll0;
    float2 llllllllllll8 = lllllllll1;
    float llllllllllllllllllllllllll5 = hatchingData.size / 2;
    float llllllllllllll8 = llllllllllll8.x;
    float lllllllllllllll8 = llllllllllll8.y; 
    float llllllllllllllll8 = max(hatchingData.dashDensity, 0.0001);
    llllllllllllll8 *= hatchingData.density;
    lllllllllllllll8 *= llllllllllllllll8; 
    float lllllllllllllllll8 = floor(llllllllllllll8); 
    float llllllllllllllllll8 = lllllllllllllll8; 
    float lllllllllllllllllll8 = lllllllllll8 ? fwidth(llllllllllllll8) : 0.0;
    if (stylingRandomData.enableRandomizer == 1)
    {
        llllllllllllll8 += noiseSampleData.perlinNoise * stylingRandomData.noiseIntensity;
        lllllllllllllll8 += noiseSampleData.perlinNoise * stylingRandomData.noiseIntensity; 
        llllllllllllllllll8 = lllllllllllllll8; 
        lllllllllllllllllll8 = lllllllllll8 ? fwidth(llllllllllllll8) : 0.0;
        float llllllllllllllllllll8 = 0;
        if (stylingRandomData.thicknessRandomMode == 0)
        {
            llllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.thicknessRandomMode == 1) 
        {
            llllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            llllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        llllllllllllllllllll8 *= stylingRandomData.thicknesshRandomIntensity;
        float lllllllllllllllllllll8 = remap(0, 1, 0.0, llllllllllllllllllllllllll5, llllllllllllllllllll8);
        llllllllllllllllllllllllll5 -= lllllllllllllllllllll8;
        float llllllllllllllllllllll8 = 0;
        if (stylingRandomData.spacingRandomMode == 0)
        {
            llllllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.spacingRandomMode == 1) 
        {
            llllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            llllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        float lllllllllllllllllllllll8 = llllllllllllllllllllllllll5;
        if (hatchingData.style == 0 && hatchingData.dashEnabled == 1 && hatchingData.dashType == 1)
        {
            lllllllllllllllllllllll8 = CalculateHatchingDashSafeSpacingHalfWidth(
                llllllllllllllllllllllllll5,
                hatchingData.hardness,
                lllllllllllllllllll8,
                lllllllllll8
            );
        }
        float llllllllllllllllllllllll8 = remap(0, 1, -0.5 + lllllllllllllllllllllll8, 0.5 - lllllllllllllllllllllll8, llllllllllllllllllllll8);
        llllllllllllll8 += llllllllllllllllllllllll8 * stylingRandomData.spacingRandomIntensity * saturate(1 - stylingRandomData.noiseIntensity); 
    }
    llllllllllllll8 = abs(frac(llllllllllllll8) - 0.5);
    float llllllllllllllllll5 = 0;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllllllllll8 = 0;
        if (stylingRandomData.lengthRandomMode == 0)
        {
            llllllllllllllllllllllllll8 = noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity); 
        }
        else if (stylingRandomData.lengthRandomMode == 1)
        {
            llllllllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored; 
        }
        else
        {
            llllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity))) / 2); 
        }
        float lllllllllllllllllllllllllll8 = llllllllllllllllllllllllll8 * stylingRandomData.lengthRandomIntensity;
        llllllllllllllllll5 = remap(0, 1 - lllllllllllllllllllllllllll8, 0, 1, ll0);
    }
    else
    {
        llllllllllllllllll5 = remap(0, 1, 0, 1, ll0);
    }
    float llllllllllllllllllllllllllll8 = smoothstep(min(1 - hatchingData.sizeFalloff, 0.99), 1, llllllllllllllllll5);
    llllllllllllllllllllllllllll8 = max(llllllllllllllllllllllllll5 - llllllllllllllllllllllllllll8, 0);
    float lllllllllllllllllllllllllllll8 = hatchingData.hardness; 
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllllllllllllll8 = 0;
        if (stylingRandomData.hardnessRandomMode == 0) 
        {
            llllllllllllllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.hardnessRandomMode == 1) 
        {
            llllllllllllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else
        {
            llllllllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        lllllllllllllllllllllllllllll8 = min(saturate(hatchingData.hardness - llllllllllllllllllllllllllllll8 * stylingRandomData.hardnessRandomIntensity), hatchingData.hardness); 
    }
    float lllllllllllllllllll6 = llllllllllllll8; 
    llllllllllllll8 = CalculateHatching1DMaskFromDistance( 
        lllllllllllllllllll6,
        llllllllllllllllllllllllllll8,
        lllllllllllllllllllllllllllll8,
        llllllllllllllllllllllllll5,
        hatchingData.size,
        lllllllllllllllllll8,
        lllllllllll8
    );
    if (hatchingData.style == 0 && hatchingData.dashEnabled == 1)
    {
        float l9 = CalculateHatchingDashContinuity(
            hatchingData.dashTransitionPosition,
            hatchingData.dashTransitionSoftness,
            llllllllllllllllll5
        ); 
        float ll9 = saturate(hatchingData.dashLength) * 0.5; 
        float llllllllllllllllllllll6 = lerp(ll9, 0.5, l9); 
        lllllllllllllll8 += lllllllllllllllll8 * saturate(hatchingData.dashOffset); 
        float llll9 = lllllllllll8 ? fwidth(llllllllllllllllll8) : 0.0; 
        float llllllllllllllllllll6 = abs(frac(lllllllllllllll8) - 0.5); 
        llllllllllllll8 = ApplyHatchingDashMode( 
            llllllllllllll8,
            lllllllllllllllllll6,
            llllllllllllllllllll6,
            llllllllllllllllllllllllllll8,
            llllllllllllllllllllll6,
            lllllllllllllllllllllllllllll8,
            lllllllllllllllllll8,
            llll9,
            hatchingData.dashType,
            hatchingData.dashRoundness,
            lllllllllll8
        );
    }
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllll9;
        if (stylingRandomData.opacityRandomMode == 0) 
        {
            llllll9 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.opacityRandomMode == 1) 
        {
            llllll9 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else 
        {
            llllll9 = ((noiseSampleData.perlinNoiseFloored * 5) + noiseSampleData.whiteNoise) / 2;
            llllll9 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        llllllllllllll8 = saturate(llllllllllllll8 - (llllll9 * stylingRandomData.opacityRandomIntensity));
    }
    float lllllll9 = smoothstep(min(1 - hatchingData.opacityFalloff, 0.99), 1, llllllllllllllllll5);
    llllllllllllll8 *= 1 - lllllll9;
    llllllllllllll8 *= hatchingData.opacity;
    return llllllllllllll8;
}
float Halftones(
float ll0, float2 lllllllll1, StylingData halftonesData, StylingRandomData stylingRandomData, NoiseSampleData noiseSampleData, half lllllllllllllllllllllllllllll5
)
{
    float2 llllllllllll9 = lllllllll1;
    llllllllllll9 *= halftonesData.density;
    if (stylingRandomData.enableRandomizer == 1)
    {
        llllllllllll9 += noiseSampleData.perlinNoise * stylingRandomData.noiseIntensity;
    }
    if (fmod(floor(llllllllllll9.y), 2) == 0)
    {
        llllllllllll9.x += halftonesData.offset;
    }
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllllllllll8 = 0;
        if (stylingRandomData.lengthRandomMode == 0)
        {
            llllllllllllllllllllllllll8 = noiseSampleData.whiteNoiseFloored * saturate(1 - stylingRandomData.noiseIntensity); 
        }
        else if (stylingRandomData.lengthRandomMode == 1)
        {
            llllllllllllllllllllllllll8 = noiseSampleData.perlinNoise; 
        }
        else
        {
            llllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoise + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity))) / 2); 
        }
        float lllllllllllllllllllllllllll8 = llllllllllllllllllllllllll8 * stylingRandomData.lengthRandomIntensity;
        ll0 -= lllllllllllllllllllllllllll8;
    }
    float lllllllllllllll9 = halftonesData.size;
    float llllllllllllllll9 = min(saturate(halftonesData.sizeMin), saturate(halftonesData.size)) / 2;
    if (halftonesData.sizeControl == 1)  
    {
        lllllllllllllll9 *= ll0;
    }
    else
    {
        float lllllllllllllllll9 = smoothstep(min(1 - halftonesData.sizeFalloff, 1), 1, (1 - ll0)); 
        lllllllllllllll9 = max(lllllllllllllll9 - lllllllllllllllll9, 0);
    }
    lllllllllllllll9 /= 2;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllll8 = 0;
        if (stylingRandomData.thicknessRandomMode == 0)
        {
            llllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.thicknessRandomMode == 1) 
        {
            llllllllllllllllllll8 = noiseSampleData.perlinNoise;
        }
        else 
        {
            llllllllllllllllllll8 = ((noiseSampleData.perlinNoise) + noiseSampleData.whiteNoise) / 2;
        }
        float lllllllllllllllllll9 = remap(0, 1, 0.0, lllllllllllllll9, llllllllllllllllllll8 * stylingRandomData.thicknesshRandomIntensity);
        lllllllllllllll9 -= lllllllllllllllllll9;
    }
    lllllllllllllll9 = max(lllllllllllllll9, llllllllllllllll9);
    float llllllllllllllllllll9 = 1 - halftonesData.roundness;
    float lllllllllllllllllllll9 = smoothstep(halftonesData.roundnessFalloff, 1, 1 - ll0);
    llllllllllllllllllll9 = max(llllllllllllllllllll9 - lllllllllllllllllllll9 * 4, 0);
    llllllllllllllllllll9 /= 2;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllllll8 = 0;
        if (stylingRandomData.spacingRandomMode == 0)
        {
            llllllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.spacingRandomMode == 1) 
        {
            llllllllllllllllllllll8 = noiseSampleData.perlinNoise;
        }
        else 
        {
            llllllllllllllllllllll8 = ((noiseSampleData.perlinNoise) + noiseSampleData.whiteNoise) / 2;
        }
        float llllllllllllllllllllllll8 = remap(0, 1, -0.5 + lllllllllllllll9, 0.5 - lllllllllllllll9, llllllllllllllllllllll8);
        llllllllllll9 += llllllllllllllllllllllll8 * stylingRandomData.spacingRandomIntensity * saturate(1 - stylingRandomData.noiseIntensity); 
    }
    float llllllllllllllllllllllll9 = halftonesData.hardness;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllllllllllllllllllllllllllll8 = 0;
        if (stylingRandomData.hardnessRandomMode == 0) 
        {
            llllllllllllllllllllllllllllll8 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.hardnessRandomMode == 1) 
        {
            llllllllllllllllllllllllllllll8 = noiseSampleData.perlinNoise * 5;
        }
        else
        {
            llllllllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoise + noiseSampleData.whiteNoise) / 2) * 5;
        }
        llllllllllllllllllllllll9 = min(saturate(halftonesData.hardness - llllllllllllllllllllllllllllll8 * stylingRandomData.hardnessRandomIntensity), halftonesData.hardness);
    }
    float llllllllllllllllllllllllll9 = remap(0, 1, 0, lllllllllllllll9, llllllllllllllllllllllll9);
    float l1 = length(max(abs(frac(llllllllllll9) - 0.5) - llllllllllllllllllll9 * llllllllllllllllllllllllll9 * 2, 0.0)) + llllllllllllllllllll9 * llllllllllllllllllllllllll9 * 2;
    float llllllllllllllllllllllllllll9 = max(lllllllllllllll9 - llllllllllllllllllllllllll9, 0.0);
    if (lllllllllllllllllllllllllllll5)
    {
        llllllllllllllllllllllllllll9 = max(llllllllllllllllllllllllllll9, min(fwidth(l1), lllllllllllllll9));
    }
    float lllllllllllllllllllllllllllll9 = 1 - smoothstep(lllllllllllllll9 - llllllllllllllllllllllllllll9, lllllllllllllll9, l1);
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llllll9;
        if (stylingRandomData.opacityRandomMode == 0) 
        {
            llllll9 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.opacityRandomMode == 1) 
        {
            llllll9 = noiseSampleData.perlinNoise * 5;
        }
        else 
        {
            llllll9 = ((noiseSampleData.perlinNoise * 5) + noiseSampleData.whiteNoise) / 2;
            llllll9 = ((noiseSampleData.perlinNoise + noiseSampleData.whiteNoise) / 2) * 5;
        }
        lllllllllllllllllllllllllllll9 = saturate(lllllllllllllllllllllllllllll9 - (llllll9 * stylingRandomData.opacityRandomIntensity));
    }
    float lllllllllllllllllllllllllllllll9 = smoothstep(min(1 - halftonesData.opacityFalloff, 0.99), 1, 1 - ll0);
    if (halftonesData.type == 1 || halftonesData.opacityFalloff != 0)
    {
        lllllllllllllllllllllllllllll9 *= 1 - lllllllllllllllllllllllllllllll9;
    }
    lllllllllllllllllllllllllllll9 *= halftonesData.opacity;
    lllllllllllllllllllllllllllll9 = 1 - lllllllllllllllllllllllllllll9;
    return lllllllllllllllllllllllllllll9;
}
void DoBlending(
inout float4 l10, float ll0, float lll10, float4 llll10
)
{
    if (lll10 == 0) 
    {
        l10 = lerp(l10, llll10, ll0);
    }
    else if (lll10 == 1) 
    {
        l10 += (llll10 * ll0);
    }
    else if (lll10 == 2) 
    {
        l10 *= 1 - ll0 + (llll10 * ll0); 
    }
    else if (lll10 == 3) 
    {
        l10 -= (llll10 * ll0);
    }
    else if (lll10 == 4) 
    {
        l10 += llll10 * (1.0 - l10) * ll0;
    }
    else if (lll10 == 5) 
    {
        float4 lllll10 = 2.0 * llll10 - 1.0;
        float4 llllll10 = l10 * lllll10;
        float4 lllllll10 = (1.0 - l10) * lllll10;
        l10 += lerp(llllll10, lllllll10, step(0.5, l10)) * ll0;
    }
    else if (lll10 == 6) 
    {
        l10 += (2.0 * llll10 - 1.0) * l10 * (1.0 - l10) * ll0;
    }
    else if (lll10 == 7) 
    {
        l10 += max(llll10 - l10, 0.0) * ll0;
    }
    else if (lll10 == 8) 
    {
        l10 += min(llll10 - l10, 0.0) * ll0;
    }
    else if (lll10 == 9) 
    {
        float4 llllllll10 = l10 / max(llll10, 0.0001);
        l10 = lerp(l10, llllllll10, ll0);
    }
}
void DoToonShading(
#if _URP
    InputData inputData, 
    SurfaceData surface,
#else
#if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
                 SurfaceOutputStandardSpecular o,
#elif _BDRFLAMBERT || _BDRF3 || _SIMPLELIT
                 SurfaceOutput o,
#else
                 SurfaceOutputStandard o,
#endif
    UnityGI gi,
#if !_PASSFORWARDADD
    UnityGIInput giInput,
#endif
#endif
    ShaderData d,
#if _URP
#if UNITY_VERSION >= 202120
    float3 lllllllllllllll10,
#endif
#endif
    inout float4 l10,
    int lllllllllllllllllllll10, float llllllllllllllllllllll10,
    half lllllllllllllllllllllll10,
    half llllllllllllllllllllllll10,
    float2 lllllllll1, float4 llllllllllllllllllllllllllll0,
    sampler2D lllllllllllllllllllllllllll10,
    half llllllllllllllllllllllllllll10,
    half lllllllllllllllllllllllllllll10,
    half llllllllllllllllllllllllllllll10, half lllllllllllllllllllllllllllllll10,
#ifdef USE_UNITY_TEXTURE_2D_TYPE
    UnityTexture2D ll11,
#else
    sampler2D ll11,
    float4 lll11,
#endif
    half llll11,
    half lllll11, float llllll11,
    half lllllll11, float4 llllllll11,
    float lllllllll11, float llllllllll11, float lllllllllll11, float4 llllllllllll11,
    float lllllllllllll11, float llllllllllllll11, float lllllllllllllll11, half llllllllllllllll11, float4 lllllllllllllllll11,
    half llllllllllllllllll11,
    half lllllllllllllllllll11, half llllllllllllllllllll11, float4 lllllllllllllllllllll11, float llllllllllllllllllllll11, float lllllllllllllllllllllll11, float llllllllllllllllllllllll11, half lllllllllllllllllllllllll11, half llllllllllllllllllllllllll11,
    half lllllllllllllllllllllllllll11, half llllllllllllllllllllllllllll11, float4 lllllllllllllllllllllllllllll11, float llllllllllllllllllllllllllllll11, float lllllllllllllllllllllllllllllll11, float l12, half ll12, half lll12,
    half llll12,
    UVSets uvSets,
    GeneralStylingData generalStylingData,
    half lllll12, half lllllllllll8,
    half lllllll12,
    half llllllll12,
    float lllllllll12, float llllllllll12,
    half lllllllllll12,
    half llllllllllll12,
    half lllllllllllll12, float llllllllllllll12,
    PositionAndBlendingData positionAndBlendingDataShading, UVSpaceData uvSpaceDataShading, StylingData stylingDataShading, StylingRandomData stylingRandomDataShading,
    half lllllllllllllll12,
    half llllllllllllllll12,
    half lllllllllllllllll12, float llllllllllllllllll12,
    half lllllllllllllllllll12, float llllllllllllllllllll12,
    PositionAndBlendingData positionAndBlendingDataCastShadows, UVSpaceData uvSpaceDataCastShadows, StylingData stylingDataCastShadows, StylingRandomData stylingRandomDataCastShadows,
    half lllllllllllllllllllll12,
    half llllllllllllllllllllll12, float lllllllllllllllllllllll12, float llllllllllllllllllllllll12, half lllllllllllllllllllllllll12, half llllllllllllllllllllllllll12,
    half lllllllllllllllllllllllllll12,
    half llllllllllllllllllllllllllll12, float lllllllllllllllllllllllllllll12,
    PositionAndBlendingData positionAndBlendingDataSpecular, UVSpaceData uvSpaceDataSpecular, StylingData stylingDataSpecular, StylingRandomData stylingRandomDataSpecular,
    half llllllllllllllllllllllllllllll12,
    half lllllllllllllllllllllllllllllll12, float l13, float ll13, half lll13,
    half llll13,
    half lllll13,
    half llllll13, float lllllll13,
    PositionAndBlendingData positionAndBlendingDataRim, UVSpaceData uvSpaceDataRim, StylingData stylingDataRim, StylingRandomData stylingRandomDataRim,
#ifdef USE_UNITY_TEXTURE_2D_TYPE
    UnityTexture2D lllllllllllllllllllllllllllll7, UnityTexture2D llllllllllllllllllllllllllllll7, 
#else
    sampler2D lllllllllllllllllllllllllllll7, sampler2D llllllllllllllllllllllllllllll7,
    float4 llllllllll13,
#endif
    float3 lllllllllll13
)
{
    float4 lllllllllllll13 = float4(0, 0, 0, 0);
#ifdef USE_UNITY_TEXTURE_2D_TYPE
    lllllllllllll13 = ll11.texelSize;
#else
    lllllllllllll13 = lll11;
#endif
#if _URP
        AlphaDiscard(surface.alpha, 0.5);
#else
#endif
    float llllllllllllll13 = 0;
    float4 lllllllllllllll13 = l10;
    int llllllllllllllll13 = lllllllllllllllllllll10;
#if _USE_OPTIMIZATION_DEFINES
    #if _ENABLE_TOON_SHADING
        llllllllllllllllllllllllllllll10 = 1;
    #else
        llllllllllllllllllllllllllllll10 = 0;
    #endif
        #if _SHADING_COLOR
            llllllllllllllllllllllllllll10 = 0;
        #else
            llllllllllllllllllllllllllll10 = 1;
        #endif 
    #if _ENABLE_STYLING
        llll12 = 1;
    #else
        llll12 = 0;
    #endif
    #if _ENABLE_SHADING_STYLING
        lllllll12 = 1;
    #else
        lllllll12 = 0;
    #endif
        #if _URP
            #ifdef _LIGHT_SOURCE
                    _LightSource = _LIGHT_SOURCE;
            #endif     
        #endif
        #if _ENABLE_CASTSHADOWS_STYLING
                lllllllllllllll12 = 1;
        #else
                lllllllllllllll12 = 0;
        #endif
        #ifdef _STYLING_CASTSHADOWS_SYNC_WITH_OTHER_STYLING
                llllllllllllllll12 = _STYLING_CASTSHADOWS_SYNC_WITH_OTHER_STYLING;
        #endif  
        #if _SHADING_TERMINATORPOSITION
                lllllllll11 = lllllllll11;
        #else
                lllllllll11 = 0;
        #endif
        #if _SHADING_STYLING_TERMINATORPOSITION
                lllllllllll12 = lllllllllll12;
        #else
                lllllllllll12 = 0;
        #endif    
        #ifdef _SHADING_STYLING_UVSET
                _UVSet = _SHADING_STYLING_UVSET;
        #endif 
        #ifdef _CASTSHADOWS_STYLING_UVSET
                _CastShadowsUVSet = _CASTSHADOWS_STYLING_UVSET;
        #endif 
        #ifdef _SPECULAR_STYLING_UVSET
                _SpecularUVSet = _SPECULAR_STYLING_UVSET;
        #endif 
        #ifdef _RIM_STYLING_UVSET
                _RimUVSet = _RIM_STYLING_UVSET;
        #endif 
    #if _ENABLE_SPECULAR_STYLING
        lllllllllllllllllllll12 = 1;
    #else
        lllllllllllllllllllll12 = 0;
    #endif
    #if _ENABLE_SPECULAR
        lllllllllllllllllll11 = 1;
    #else
        lllllllllllllllllll11 = 0;
    #endif
        #if _SUM_LIGHTS_BEFORE_POSTERIZATION
            lllllllllllllllllllllll10 = 1;
        #else
            lllllllllllllllllllllll10 = 0;
        #endif
    #if _SHADING_USE_LIGHT_COLORS
        llllllllllllllllllllllll10 = 1;
    #else
        llllllllllllllllllllllll10 = 0;
    #endif
    #if _SPECULAR_USE_LIGHT_COLORS
        llllllllllllllllllllllllll11 = 1;
    #else
        llllllllllllllllllllllllll11 = 0;
    #endif
    #if _STYLING_SPECULAR_USE_LIGHT_COLORS
        llllllllllllllllllllllllll12 = 1;
    #else
        llllllllllllllllllllllllll12 = 0;
    #endif  
    #if _SHADING_STYLING_ENABLE_DASHES
            _StylingShadingEnableDashes = 1;
    #else
           _StylingShadingEnableDashes = 0;
    #endif
    #ifdef _SHADING_STYLING_DASHES_TYPE
            _StylingShadingDashesType = _SHADING_STYLING_DASHES_TYPE;
    #endif
    #if _CASTSHADOWS_STYLING_ENABLE_DASHES
            _StylingCastShadowsEnableDashes = 1;
    #else
           _StylingCastShadowsEnableDashes = 0;
    #endif
    #ifdef _CASTSHADOWS_STYLING_DASHES_TYPE
            _StylingCastShadowsDashesType = _CASTSHADOWS_STYLING_DASHES_TYPE;
    #endif
    #if _SPECULAR_STYLING_ENABLE_DASHES
            _StylingSpecularEnableDashes = 1;
    #else
            _StylingSpecularEnableDashes = 0;
    #endif
    #ifdef _SPECULAR_STYLING_DASHES_TYPE
            _StylingSpecularDashesType = _SPECULAR_STYLING_DASHES_TYPE;
    #endif
    #if _RIM_STYLING_ENABLE_DASHES
            _StylingRimEnableDashes = 1;
    #else
           _StylingRimEnableDashes = 0;
    #endif
    #ifdef _RIM_STYLING_DASHES_TYPE
            _StylingRimDashesType = _RIM_STYLING_DASHES_TYPE;
    #endif
    #if _SHADING_STYLING_USE_CONTROLMAP_THICKNESS
            _StylingShadingUseControlMapThickness = 1;
    #else
           _StylingShadingUseControlMapThickness = 0;
    #endif
    #if _SHADING_STYLING_ENABLE_PIXELATION
            lllllllllllll12 = 1;
    #else
           lllllllllllll12 = 0;
    #endif
    #if _CASTSHADOWS_STYLING_ENABLE_PIXELATION
            lllllllllllllllllll12 = 1;
    #else
           lllllllllllllllllll12 = 0;
    #endif
    #if _SPECULAR_STYLING_ENABLE_PIXELATION
           llllllllllllllllllllllllllll12 = 1;
    #else
           llllllllllllllllllllllllllll12 = 0;
    #endif
    #if _RIM_STYLING_ENABLE_PIXELATION
           llllll13 = 1;
    #else
           llllll13 = 0;
    #endif
    #ifdef _SPECULAR_STYLING_SHADING_INTERACTION
            lllllllllllllllllllllllll12 = _SPECULAR_STYLING_SHADING_INTERACTION;
    #endif
    #ifdef _RIM_STYLING_SHADING_INTERACTION
            llll13 = _RIM_STYLING_SHADING_INTERACTION;
    #endif
#endif
    float3 lllllllllllllllllll13;
    if (llllllllllllllllll11 == 0)
    {
        lllllllllllllllllll13 = lllllllllll13;
    }
    else
    {
#if _URP 
        lllllllllllllllllll13 = inputData.normalWS;
#else
        lllllllllllllllllll13 = o.Normal;
#endif
    }
    float3 llllllllllll2;
    if (lllllllllllllllllllllllll11 == 0)
    {
        llllllllllll2 = lllllllllll13;
    }
    else
    {
#if _URP 
        llllllllllll2 = inputData.normalWS;
#else
        llllllllllll2 = o.Normal;
#endif
    }
    float3 lllllllllllllllllllll13;
    if (lllll12 == 0)
    {
        lllllllllllllllllllll13 = lllllllllll13;
    }
    else
    {
#if _URP 
        lllllllllllllllllllll13 = inputData.normalWS;
#else
        lllllllllllllllllllll13 = o.Normal;
#endif        
    }
    float3 lll2 = normalize(d.worldSpaceViewDir);
    float4 lllllllllllllllllllllllllll13 = 0;
    float llllll2 = -1;
    float lllllllllllllllllllllllllllll13 = -1;
    half3 llllllllllllllllllllllllllllll13 = 0;
    float lllllllllll4 = 0; 
    float l14 = 0; 
    float lllllll2 = 0;
    half3 lllllllllllllllllllllllll3 = 0;
    float llll14 = 0;
    half3 lllll14 = 0;
    float llllll14 = 0;
    ToonShadingData toonShadingData;
    toonShadingData.enableToonShading = llllllllllllllllllllllllllllll10;
#if _URP
    toonShadingData.normalWS = inputData.normalWS;
#endif
    toonShadingData.normalWSNoMap = lllllllllll13;
    toonShadingData.cellTransitionSmoothness = llllllllllllllllllllll10;
    toonShadingData.numberOfCells = llllllllllllllll13;
    toonShadingData.specularEdgeSmoothness = lllllllllllllllllllllll11;
    toonShadingData.shadingAffectByNormalMap = llllllllllllllllll11;
    toonShadingData.specularAffectedByNormalMap = lllllllllllllllllllllllll11;
#if _URP
    if ((llllllllllllllllllllllllllll10 == 0 && llllllllllllllllllllllllllllll10 == 1 && (lllllll11 == 1 || lllllllllllllllllll11 == 1 || lllllllllllll11 == 1)) || (llll12 == 1 && (lllllll12 == 1 || lllllllllllllll12 == 1 || lllllllllllllllllllll12 == 1)))
    {
        if (_LightSource != 1)
        {
            bool lllllll14 = llllllllllllllllllllllllllll10 == 0 && llllllllllllllllllllllllllllll10 == 1;
            bool llllllll14 = llll12 == 1 && (lllllll12 == 1 || lllllllllllllll12 == 1 || lllllllllllllllllllll12 == 1);
            bool lllllllll14 = llllllllllllllllll11 == lllll12; 
            bool llllllllll14 = lllllllllllllllllllllllll11 == lllll12; 
            float lllllllllll14 = 1;
            float llllllllllll14 = 1;
            Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
            MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
            float lllllllllllll14 = max(mainLight.color.x, mainLight.color.y); 
            lllllllllllll14 = max(lllllllllllll14, mainLight.color.z);
            float3 lllllllllllllllll13 = lllllllllllllllllll13;
            float llll2 = llllllllllllllllllllll11;
            float lllll2 = lllllllllllllllllllllll11;
            float lllllllllllllllllllllll3 = llllllllllllllllllllllll11;
            float llllllllllllllllll14 = llllllllllllllllllllllllll11;
            half lllllllllllllllllll14 = lllllllllllllllllll11;
            half llllllllllllllllllll14 = lllllll11;
            if (!lllllll14)
            {
                lllllllllllllllll13 = lllllllllllllllllllll13;
                llllllllllll2 = lllllllllllllllllllll13;
                llll2 = lllllllllllllllllllllll12;
                lllll2 = llllllllllllllllllllllll12;
                lllllllllllllllllllllll3 = _StylingSpecularOpacity;
                llllllllllllllllll14 = llllllllllllllllllllllllll12;
                lllllllllllllllllll14 = lllllllllllllllllllll12;
                llllllllllllllllllll14 = lllllll12;
                lllllllll11 = lllllllllll12;
            }
            else
            {
                if (lllllll11 == 0)
                {
                    lllllllllllllllll13 = lllllllllllllllllllll13;
                    llllllllllllllllllll14 = lllllll12;
                }
                if (lllllllllllllllllll11 == 0)
                {
                    llllllllllll2 = lllllllllllllllllllll13;
                    lllllllllllllllllll14 = lllllllllllllllllllll12;
                }
                else
                {
                    if (llllllll14 && lllllllllllllllllllll12 == 1 && llllllllllllllllllllll12 == 1)
                    {
                        lllllllllllllllllllllll12 = llllllllllllllllllllll11;
                        llllllllllllllllllllllll12 = lllllllllllllllllllllll11;
                    }
                }
            }
            float lllllllllllllllllllll14 = 1;
            if (mainLight.color.r > 0.0 || mainLight.color.g > 0.0 || mainLight.color.b > 0.0)
            {
                lllllllllllllllllllll14 = (mainLight.shadowAttenuation * mainLight.distanceAttenuation);
                half llllllllllllllllllllll14 = mainLight.distanceAttenuation * lllllllllllll14;
                llllll2 = dot(mainLight.direction, lllllllllllllllll13);
                if (lllllllll11 != 0.0)
                {
                    llllll2 = shiftLinear(llllll2, max(-0.9999, lllllllll11));
                }
                if (llllll2 > 0)
                {
                    llllll2 *= llllllllllllllllllllll14;
                }
                if (lllllllllllllllllll14 || (!lllllll14 && lllllllllllllllllllll12))
                {
                    lllllll2 = CalculateSpecularMask(llllllllllll2, mainLight.direction, lll2, llll2, lllll2, llllll2); 
                    lllllll2 *= lllllllllllllllllllllll3;
                    if ((lllllll14 && lllllllllllll11) || (llll12 && lllllllllllllll12))
                    {
                        lllllll2 = min(lllllll2, mainLight.shadowAttenuation);
                    }
                    if (llllllllllllllllll14 == 1)
                    {
                        lllllllllllllllllllllllll3 = lllllll2 * mainLight.color;
                    }
                }
                if (!lllllll14)
                {
                    lllllllllllllllllllllllllllll13 = llllll2;
                    llll14 = lllllll2;
                    lllll14 = lllllllllllllllllllllllll3;
                    lllllll2 = 0;
                    lllllllllllllllllllllllll3 = 0;
                }
                else
                {
                    if (lllllll11 == 0)
                    {
                        lllllllllllllllllllllllllllll13 = llllll2;
                    }
                    if (lllllllllllllllllll11 == 0)
                    {
                        llll14 = lllllll2;
                        lllll14 = lllllllllllllllllllllllll3;
                        lllllll2 = 0;
                        lllllllllllllllllllllllll3 = 0;
                    }
                }
                if (llllllll14 && lllllll14)
                {
                    if (lllllllll14 && lllllllll12)
                    {
                        lllllllllllllllllllllllllllll13 = llllll2;
                    }
                    else
                    {
                        if (lllllllll12)
                        {
                            lllllllllll12 = lllllllll11;
                        }
                        lllllllllllllllllllllllllllll13 = dot(mainLight.direction, lllllllllllllllllllll13);
                        if (lllllllllll12 != 0.0)
                        {
                            lllllllllllllllllllllllllllll13 = shiftLinear(lllllllllllllllllllllllllllll13, max(-0.9999, lllllllllll12));
                        }
                        if (lllllllllllllllllllllllllllll13 > 0)
                        {
                            lllllllllllllllllllllllllllll13 *= llllllllllllllllllllll14; 
                        }
                    }
                    if (lllllllllllllllllllll12 == 1)
                    {
                        if (lllllllll14 && llllllllll14 && llllllllllllllllllllll12 == 1)
                        {
                            llll14 = lllllll2;
                            lllll14 = lllllllllllllllllllllllll3;
                        }
                        else
                        {
                            llll14 = CalculateSpecularMask(lllllllllllllllllllll13, mainLight.direction, lll2, lllllllllllllllllllllll12, llllllllllllllllllllllll12, lllllllllllllllllllllllllllll13);
                            if (lllllllllllll11 || lllllllllllllll12)
                            {
                                llll14 = min(llll14, mainLight.shadowAttenuation);
                            }
                            if (llllllllllllllllllllllllll12 == 1)
                            {
                                lllll14 = llll14 * mainLight.color;
                            }
                        }
                    }
                }
            {
                    lllllllllll14 = lllllllllllllllllllll14;
                }
            }
            else
            {
                lllllllllll14 = 1;
                lllllllllllllllllllll14 = 1;
                llllll2 = -1;
                lllllllllllllllllllllllllllll13 = -1;
            }
            float lllllllllllllllllllllll14 = 0;
            float llllllllllllllllllllllll14 = 0;
            float lllllllllllllllllllllllll14 = 0;
            float llllllllllllllllllllllllll14 = 0;
            float lllllllllllllllllllllllllll14 = 2;
            float llllllllllllllllllllllllllll14 = 2;
            float lllllllllllllllllllllllllllll14 = 0;
            float llllllllllllllllllllllllllllll14 = 1;
#if defined(_ADDITIONAL_LIGHTS)  
#if UNITY_VERSION >= 202200
        uint meshRenderingLayers = GetMeshRenderingLayer();
#else
            uint meshRenderingLayers = GetMeshRenderingLightLayer();
#endif
#if USE_CLUSTER_LIGHT_LOOP
        [loop]
            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                Light addLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1, 1, 1, 1));
#ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
#endif
            {
                float lllllllllllllllllllllllllllll3 = max(addLight.color.x, addLight.color.y);
                lllllllllllllllllllllllllllll3 = max(lllllllllllllllllllllllllllll3, addLight.color.z);
                half l15 = addLight.distanceAttenuation * lllllllllllllllllllllllllllll3;
                float ll15 = smoothstep(0, 0.1 / (addLight.distanceAttenuation * lllllllllllllllllllllllllllll3), addLight.distanceAttenuation * lllllllllllllllllllllllllllll3);
                float lll15 = smoothstep(0, 0.01, addLight.distanceAttenuation * lllllllllllllllllllllllllllll3);
                lllllllllllllllllllllllllllll14 += addLight.shadowAttenuation * l15;
                float llll15 = dot(addLight.direction, lllllllllllllllll13);
                if (lllllllll11 != 0)
                {
                    llll15 = shiftLinear(llll15, max(-0.9999, lllllllll11));
                }
                float lllll15 = lerp(-1, llll15, ll15);
        {
                    llllllllllllllllllllllllllllll14 = min(llllllllllllllllllllllllllllll14, lerp(1, addLight.shadowAttenuation, lll15));
                }
                float llllll15 = saturate(lllll15) * l15; 
                lllllllllllllllllllllllll14 += llllll15;
                if (lllllll14 || (lllllll12 == 1 && lllllllllllllll12 == 1 && llllllllllllllll12 == 1))
                {
                    if (lllllllllllll11 == 1 || (!lllllll14))
                    {
                        llllll15 *= addLight.shadowAttenuation;
                    }
                    lllllllllllllllllllllll14 += llllll15;
                }
                if (lllllll14)
                {
                    if (llllllllllllllllllllllll10 == 1)
                    {
                        llllllllllllllllllllllllllllll13 += saturate(llllll15 * (addLight.color));
                    }
                }
                if (sign(lllll15) == -1 && lllllllllllllllllllllllll14 == 0)
                {
                    float lllllll15 = abs(lllll15);
                    lllllllllllllllllllllllllll14 = min(lllllllllllllllllllllllllll14, lllllll15);
                }
                float llllllll15 = 0;
                if (lllllllllllllllllll11 || (!lllllll14 && lllllllllllllllllllll12))
                {
                    llllllll15 = CalculateSpecularMask(llllllllllll2, addLight.direction, lll2, llll2, lllll2, llll15);
                    llllllll15 *= lllllllllllllllllllllll3;
                    if (lllllllllllll11 || lllllllllllllll12)
                    {
                        llllllll15 *= addLight.shadowAttenuation;
                    }
                    lllllll2 += llllllll15;
                    if (llllllllllllllllll14 == 1)
                    {
                        lllllllllllllllllllllllll3 += addLight.color * llllllll15;
                    }
                }
                if (llllllll14 && lllllll14) 
                {
                    float lllllllll15 = 0;
                    if (lllllllll14 && (lllllllll12 || (lllllllll11 == lllllllllll12)))
                    {
                        llllllllllllllllllllllllll14 = lllllllllllllllllllllllll14;
                        llllllllllllllllllllllllllll14 = lllllllllllllllllllllllllll14;
                        llllllllllllllllllllllll14 = lllllllllllllllllllllll14;
                    }
                    else
                    {
                        lllllllll15 = dot(addLight.direction, lllllllllllllllllllll13);
                        if (lllllllll12)
                        {
                            lllllllllll12 = lllllllll11;
                        }
                        if (lllllllllll12 != 0)
                        {
                            lllllllll15 = shiftLinear(lllllllll15, max(-0.9999, lllllllllll12));
                        }
                        float llllllllll15 = lerp(-1, lllllllll15, ll15);
                        float lllllllllll15 = saturate(llllllllll15) * l15;
                        llllllllllllllllllllllllll14 += lllllllllll15;
                        if (lllllllllllllll12 == 1 && lllllll12 == 1 && llllllllllllllll12 == 1)
                        {
                            lllllllllll15 *= addLight.shadowAttenuation;
                            llllllllllllllllllllllll14 += lllllllllll15;
                        }
                        if (sign(llllllllll15) == -1 && llllllllllllllllllllllllll14 == 0)
                        {
                            float llllllllllll15 = abs(llllllllll15);
                            llllllllllllllllllllllllllll14 = min(llllllllllllllllllllllllllll14, llllllllllll15);
                        }
                    }
                    if (lllllllllllllllllllll12 == 1)
                    {
                        float lllllllllllll15 = 0;
                        if (lllllllll14 && llllllllll14 && llllllllllllllllllllll12 == 1)
                        {
                            llll14 = lllllll2;
                            lllllllllllll15 = llllllll15;
                        }
                        else
                        {
                            lllllllllllll15 = CalculateSpecularMask(llllllllllll2, addLight.direction, lll2, lllllllllllllllllllllll12, llllllllllllllllllllllll12, llll15);
                            lllllllllllll15 = lllllllllllll15;
                            if (lllllllllllllll12)
                            {
                                lllllllllllll15 *= addLight.shadowAttenuation;
                            }
                            llll14 += lllllllllllll15;
                        }
                        if (llllllllllllllllllllllllll12 == 1)
                        {
                            lllll14 += addLight.color * lllllllllllll15;
                        }
                    }
                }
                }
            }
#endif
            uint pixelLightCount = GetAdditionalLightsCount();
            LIGHT_LOOP_BEGIN(pixelLightCount)
            Light addLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1, 1, 1, 1));
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
#endif
        {  
                float lllllllllllllllllllllllllllll3 = max(addLight.color.x, addLight.color.y);
                lllllllllllllllllllllllllllll3 = max(lllllllllllllllllllllllllllll3, addLight.color.z);
                half l15 = addLight.distanceAttenuation * lllllllllllllllllllllllllllll3;
                float ll15 = smoothstep(0, 0.1 / (addLight.distanceAttenuation * lllllllllllllllllllllllllllll3), addLight.distanceAttenuation * lllllllllllllllllllllllllllll3);
                float lll15 = smoothstep(0, 0.01, addLight.distanceAttenuation * lllllllllllllllllllllllllllll3);
                lllllllllllllllllllllllllllll14 += addLight.shadowAttenuation * l15;
                float llll15 = dot(addLight.direction, lllllllllllllllll13);
                if (lllllllll11 != 0)
                {
                    llll15 = shiftLinear(llll15, max(-0.9999, lllllllll11));
                }
                float lllll15 = lerp(-1, llll15, ll15);
            {
                    llllllllllllllllllllllllllllll14 = min(llllllllllllllllllllllllllllll14, lerp(1, addLight.shadowAttenuation, lll15));
                }
                float llllll15 = saturate(lllll15) * l15; 
                lllllllllllllllllllllllll14 += llllll15;
                if (lllllll14 || (lllllll12 == 1 && lllllllllllllll12 == 1 && llllllllllllllll12 == 1))
                {
                    if (lllllllllllll11 == 1 || (!lllllll14))
                    {
                        llllll15 *= addLight.shadowAttenuation;
                    }
                    lllllllllllllllllllllll14 += llllll15;
                }
                if (lllllll14)
                {
                    if (llllllllllllllllllllllll10 == 1)
                    {
                        llllllllllllllllllllllllllllll13 += saturate(llllll15 * (addLight.color));
                    }
                }
                if (sign(lllll15) == -1 && lllllllllllllllllllllllll14 == 0)
                {
                    float lllllll15 = abs(lllll15);
                    lllllllllllllllllllllllllll14 = min(lllllllllllllllllllllllllll14, lllllll15);
                }
                float llllllll15 = 0;
                if (lllllllllllllllllll11 || (!lllllll14 && lllllllllllllllllllll12))
                {
                    llllllll15 = CalculateSpecularMask(llllllllllll2, addLight.direction, lll2, llll2, lllll2, llll15);
                    llllllll15 *= lllllllllllllllllllllll3;
                    if (lllllllllllll11 || lllllllllllllll12)
                    {
                        llllllll15 *= addLight.shadowAttenuation;
                    }
                    lllllll2 += llllllll15;
                    if (llllllllllllllllll14 == 1)
                    {
                        lllllllllllllllllllllllll3 += addLight.color * llllllll15;
                    }
                }
                if (llllllll14 && lllllll14) 
                {
                    float lllllllll15 = 0;
                    if (lllllllll14 && (lllllllll12 || (lllllllll11 == lllllllllll12)))
                    {
                        llllllllllllllllllllllllll14 = lllllllllllllllllllllllll14;
                        llllllllllllllllllllllllllll14 = lllllllllllllllllllllllllll14;
                        llllllllllllllllllllllll14 = lllllllllllllllllllllll14;
                    }
                    else
                    {
                        lllllllll15 = dot(addLight.direction, lllllllllllllllllllll13);
                        if (lllllllll12)
                        {
                            lllllllllll12 = lllllllll11;
                        }
                        if (lllllllllll12 != 0)
                        {
                            lllllllll15 = shiftLinear(lllllllll15, max(-0.9999, lllllllllll12));
                        }
                        float llllllllll15 = lerp(-1, lllllllll15, ll15);
                        float lllllllllll15 = saturate(llllllllll15) * l15;
                        llllllllllllllllllllllllll14 += lllllllllll15;
                        if (lllllllllllllll12 == 1 && lllllll12 == 1 && llllllllllllllll12 == 1)
                        {
                            lllllllllll15 *= addLight.shadowAttenuation;
                            llllllllllllllllllllllll14 += lllllllllll15;
                        }
                        if (sign(llllllllll15) == -1 && llllllllllllllllllllllllll14 == 0)
                        {
                            float llllllllllll15 = abs(llllllllll15);
                            llllllllllllllllllllllllllll14 = min(llllllllllllllllllllllllllll14, llllllllllll15);
                        }
                    }
                    if (lllllllllllllllllllll12 == 1)
                    {
                        float lllllllllllll15 = 0;
                        if (lllllllll14 && llllllllll14 && llllllllllllllllllllll12 == 1)
                        {
                            llll14 = lllllll2;
                            lllllllllllll15 = llllllll15;
                        }
                        else
                        {
                            lllllllllllll15 = CalculateSpecularMask(llllllllllll2, addLight.direction, lll2, lllllllllllllllllllllll12, llllllllllllllllllllllll12, llll15);
                            lllllllllllll15 = lllllllllllll15;
                            if (lllllllllllllll12)
                            {
                                lllllllllllll15 *= addLight.shadowAttenuation;
                            }
                            llll14 += lllllllllllll15;
                        }
                        if (llllllllllllllllllllllllll12 == 1)
                        {
                            lllll14 += addLight.color * lllllllllllll15;
                        }
                    }
                }
            }
            LIGHT_LOOP_END
#endif
            if (llllllllllllllllllllllllllllll10 == 1 && lllllll11 == 1 && llllllllllllllllllllllll10 == 1)
            {
                float3 llllllllllllllllllllllllllll15 = saturate(saturate(llllll2) * (mainLight.color));
                if (lllllllllllll11 == 1)
                {
                    llllllllllllllllllllllllllll15 *= lllllllllllllllllllll14;
                }
                llllllllllllllllllllllllllllll13 += saturate(llllllllllllllllllllllllllll15);
                llllllllllllllllllllllllllllll13 = saturate(llllllllllllllllllllllllllllll13);
                const float3 lllllllllllllllllllllllllllll15 = float3(0.2126, 0.7152, 0.0722);
                float llllllllllllllllllllllllllllll15 = dot(llllllllllllllllllllllllllllll13, lllllllllllllllllllllllllllll15); 
                float lllllllllllllllllllllllllllllll15 = Posterize(saturate(llllllllllllllllllllllllllllll15), toonShadingData); 
                const float l16 = 1e-6; 
                float ll16 = (llllllllllllllllllllllllllllll15 > l16) ? (lllllllllllllllllllllllllllllll15 / llllllllllllllllllllllllllllll15) : 0.0;
                llllllllllllllllllllllllllllll13 = llllllllllllllllllllllllllllll13 * ll16;
            }
            if (!lllllll14)
            {
                llllllllllllllllllllllllll14 = lllllllllllllllllllllllll14;
                llllllllllllllllllllllll14 = lllllllllllllllllllllll14;
                llllllllllllllllllllllllllll14 = lllllllllllllllllllllllllll14;
                llll14 = lllllll2 + llll14; 
                lllll14 = lllllllllllllllllllllllll3;
                lllllll2 = 0;
                lllllllllllllllllllllllll3 = 0;
            }
            float lll16 = saturate(llllll2);
            float llll16 = saturate(lllllllllllllllllllllll14);
            if (lllllllllllllllllllllllllllllll10 == 0)
            {
                if (lllllllllllllllllllllll10 == 0)
                {
                    lll16 = Posterize(lll16, toonShadingData);
                    llll16 = Posterize(llll16, toonShadingData);
                }
            }
            if (llllllllllllllllllllllllllllll10 == 1 && lllllllllllll11 == 1 && (lllllll11 == 0 || (lllllllllllllllllllllllllll11 && ll12 == 1)))
            {
                float lllll16 = saturate(min(lllllllllll14, llllllllllllllllllllllllllllll14));
                float llllll16 = lllllllllllllllllllll14 * saturate(llllll2) + saturate(lllllllllllllllllllllllll14) * lllllllllllllllllllllllllllll14;
                float lllllll16 = saturate((1 - lllll16) * saturate(llllll16)) + lllll16; 
                lllllllllll4 = lllllll16;
            }
            if (llll12 == 1)
            {
                if (lllllllllllllll12 == 1)
                {
                    if (llllllllllllllll12 == 1)
                    {
                        l14 = saturate(lllllllllllllllllllll14 + lllllllllllllllllllllllllllll14);
                        if (lllllllllllllllllllllllllllll13 > 0)
                        {
                            lllllllllllllllllllllllllllll13 = saturate(lllllllllllllllllllllllllllll13);
                            lllllllllllllllllllllllllllll13 *= lllllllllllllllllllll14;
                        }
                        if (llllllllllllllllllllllllll14 > 0)
                        {
                            lllllllllllllllllllllllllllll13 = saturate(lllllllllllllllllllllllllllll13);
                            lllllllllllllllllllllllllllll13 += saturate(llllllllllllllllllllllll14);
                        }
                        else
                        {
                            if (llllllllllllllllllllllllllll14 > 0)
                            {
                                lllllllllllllllllllllllllllll13 = max(lllllllllllllllllllllllllllll13, -1 * llllllllllllllllllllllllllll14);
                            }
                        }
                    }
                    else
                    {
                        float lllll16 = min(lllllllllll14, llllllllllllllllllllllllllllll14);
                        float llllll16 = lllllllllllllllllllll14 * saturate(lllllllllllllllllllllllllllll13) + saturate(llllllllllllllllllllllllll14) * lllllllllllllllllllllllllllll14;
                        float lllllll16 = ((1 - lllll16) * (llllll16)) + lllll16; 
                        l14 = lllllll16;
                    }
                }
                if (lllllllllllllll12 == 0 || llllllllllllllll12 != 1) 
                {
                    float lllllllllll16 = lllllllllllllllllllllllllllll13;
                    lllllllllllllllllllllllllllll13 = saturate(lllllllllllllllllllllllllllll13) + saturate(llllllllllllllllllllllllll14);
                    if (lllllllllllllllllllllllllllll13 == 0)
                    {
                        lllllllllllllllllllllllllllll13 = max(lllllllllll16, -1 * llllllllllllllllllllllllllll14);
                    }
                }
            }
            if (llllll2 > 0)
            {
                llllll2 = saturate(lll16);
                if (lllllllllllll11 == 1)
                {
                    llllll2 *= lllllllllllllllllllll14;
                }
            }
            if (lllllllllllllllllllllllll14 > 0)
            {
                llllll2 = saturate(llllll2);
                llllll2 += saturate(llll16);
            }
            else
            {
                if (lllllllllllllllllllllllllll14 > 0)
                {
                    llllll2 = max(llllll2, -1 * lllllllllllllllllllllllllll14);
                }
            }
            if (llllll2 < 0)
            {
            }
            else
            {
                if (lllllllllllllllllllllllllllllll10 == 0 && lllllllllllllllllllllll10 == 1)
                {
                    llllll2 = Posterize(saturate(llllll2), toonShadingData);
                }
            }
        }
#if defined(LIGHTMAP_ON)  || defined(DYNAMICLIGHTMAP_ON) || defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
        if(_LightSource != 0) 
        {
            const float3 lllllllllllllllllllllllllllll15 = float3(0.2126, 0.7152, 0.0722);
            const float l16 = 1e-6; 
            float3 llllllllllllll16 = (inputData.bakedGI);
            float llllllllllllllllllllllllllllll15 = dot(llllllllllllll16, lllllllllllllllllllllllllllll15); 
            l14 = saturate(l14+llllllllllllllllllllllllllllll15);
            if(_LightSource != 0 && llllllllllllllllllllllllllllll10 == 1)
            {     
                if(llllllllllllllllllllllllllll10 == 0) 
                {
                    if(lllllllllllllllllllllllllllllll10 == 0)
                    {    
                        float llllllllllllllll16 = llllllllllllllllllllllllllllll15;
                        if (lllllllll11 != 0.0) 
                        {
                            llllllllllllllll16 = shiftLinear(llllllllllllllll16, saturate(lllllllll11));
                        } 
                        llllll14 = llllllllllllllll16;
                    } 
                    else
                    {
                        if(llllllllllllllllllllllllllllll15 > 0) 
                        {
                            llllll2 = max(llllllllllllllllllllllllllllll15, llllll2);
                        }
                    }
                }    
            }
            if(_LightSource == 1)
            {
                lllllllllllllllllllllllllllll13 = 0;            
            }
            if(_LightSource != 0 && lllllll12 == 1)
            {                
                float lllllllllllllllll16 = saturate(llllllllllllllllllllllllllllll15);
                if (lllllllllll12 != 0.0) 
                {
                    lllllllllllllllll16 = shiftLinear(llllllllllllllllllllllllllllll15, saturate(lllllllllll12));
                }
                if(llllllllllllllllllllllllllllll15>0) 
                {
                    lllllllllllllllllllllllllllll13 = max(lllllllllllllllllllllllllllll13,saturate(lllllllllllllllll16));
                }     
            }
        }
#endif
    }
#else 
    UnityLight llllllllllll13 = gi.light;
    llllll2 = dot(llllllllllll13.dir, lllllllllllllllllll13);
    if (lllllllll11 != 0)
    {
        llllll2 = shiftLinear(llllll2, max(-0.9999, lllllllll11));
    }
    if (lllllllll12 == 0)
    {
        lllllllllllllllllllllllllllll13 = dot(llllllllllll13.dir, lllllllllllllllllllll13);
        if (lllllllllll12 != 0)
        {
            lllllllllllllllllllllllllllll13 = shiftLinear(lllllllllllllllllllllllllllll13, max(-0.9999, lllllllllll12));
        }
    }
    else
    {
        lllllllllllllllllllllllllllll13 = llllll2;
    }
#if !_PASSFORWARDADD    
    if (llllll2 > 0)
    {
        lllllllllll4 = giInput.atten;
    }
    else
    {
        lllllllllll4 = 1;
    }
    if (lllllllllllllll12 == 1 && lllllll12 == 1 && llllllllllllllll12 == 1)
    {
        lllllllllllllllllllllllllllll13 *= lllllllllll4;
    }
    l14 = lllllllllll4;
#else    
    lllllllllll4 = 0;    
    lllllllllllllllllllllllllll11 = 0;    
    llll12 = 0;    
    llllllllllllllllllllllllllllll12 = 0;
    lllllll12 = 0;
    lllllllllllllll12 = 0;
    stylingDataShading.color = 0;
    stylingDataSpecular.color = half4(gi.light.color,1);
#endif
#endif
    float lllllllllllllllllllll16 = lllllllllll4;
    float llllllllllllllllllllll16 = 0;
    float4 lllllllllllllllllllllll16 = 0;
    float3 lll4;
    if (lll12 == 0)
    {
        lll4 = lllllllllll13;
    }
    else
    {
#if _URP 
        lll4 = inputData.normalWS;
#else
        lll4 = o.Normal;
#endif
    }
    float lllll16 = 0;
    if (llllllllllllllllllllllllllll10 == 0) 
    {
        lllllllllllllllllllll16 = lllllllllll4;
        if (llllllllllllllllllllllllllllll10 == 1)
        {
            float3 llllllllllllllllllllllllll16 = llllllll11.rgb;
            if (lllllllllllll11 == 1
                        || (lllllll11 == 1 && lllllllllllllllllllllllllllllll10 == 0)
                        || (lllllll11 == 0 && lllllllllllllllllllllllllllllll10 == 0 && lllllllllllll11 == 1)
                        || _LightSource != 0
                        )
            {
                llllllllllllllllllllllllll16 = lerp(llllllll11.rgb, lllllllllllllll13.rgb, 1 - llllllll11.a);
            }
            if (lllllllllllllllllllllllllllllll10 == 0)
            {
                if (lllllll11 == 1)
                {
                    float llllllllllllllllllllll16 = saturate(llllll2);
#if _URP
                    float3 llllllllllllllllllllllllllll16 = 0;
                    if (_LightSource != 0)
                    {
                        float3 lllllllllllllllllllllllllllll16 = inputData.bakedGI;
                        float llllllllllllllllllllllllllllll16 = max(lllllllllllllllllllllllllllll16.r, max(lllllllllllllllllllllllllllll16.g, lllllllllllllllllllllllllllll16.b));
                        llllllllllllllllllllllllllll16 = lllllllllllllllllllllllllllll16 / max(llllllllllllllllllllllllllllll16, 1e-5); 
                    }
                    if (llllllllllllllllllllllll10 == 1)
                    {
                        if (_LightSource != 0)
                        {
                            llllllllllllllllllllllllllllll13 *= llllllllllllllllllllll16;
                            llllllllllllllllllllllllllllll13 += llllllllllllllllllllllllllll16 * saturate(llllll14);
                        }
                        l10 *= float4(llllllllllllllllllllllllllllll13, 1);                        
                    }
                    if (_LightSource != 0)
                    {
                        float lllllllllllllllllllllllllllllll16 = PosterizeMulti(saturate(llllll14), toonShadingData, 1);
                        llllllllllllllllllllll16 = saturate(llllllllllllllllllllll16 + lllllllllllllllllllllllllllllll16);
                    }
#else
                    llllllllllllllllllllll16 = Posterize(llllllllllllllllllllll16, toonShadingData);
#endif
                    l10.xyz = lerp(llllllllllllllllllllllllll16, l10.xyz, llllllllllllllllllllll16);
#if !_URP
                    if (lllllllllllll11 == 1)
                    {
                        l10 = float4(lerp(llllllllllllllllllllllllll16, l10.rgb, saturate(lllllllllll4)), lllllllllllllll13.a);
                    }
#endif
                }
            }
            else
            {
                float llllllllllllllllllllll17 = min(0.95, llllll2); 
                if (llll11 == 1 && lllllll11 == 0 && llllll2 < 0)
                {
                    llllllllllllllllllllll17 = 0;
                }
                llllllllllllllllllllll17 = (llllllllllllllllllllll17 + 1) / 2;
                float4 lllllllllllllllllllllll17 = float4(0, 0, 0, 0);
                float llllllllllllllllllllllll17 = lllllllllllll13.z;
                float lllllllllllllllllllllllll17 = llllllllllllllllllllll17 * (llllllllllllllllllllllll17 - 1);
                float2 llllllllllllllllllllllllll17 = (lllllllllllllllllllllllll17 + 0.5) * lllllllllllll13.xy;
                lllllllllllllllllllllll17 = tex2D(ll11, llllllllllllllllllllllllll17);
                DoBlending(l10, llllll11, lllll11, lllllllllllllllllllllll17);
            }
            if (lllllllllllll11 == 0 && (llll12 == 0 || lllllllllllllll12 == 0))
            {
                lllllllllll4 = 1;
            }
            if (_LightSource == 0)
            {
                if (lllllll11 == 1 && lllllllllllllllllllllllllllllll10 == 0)
                {
                    if (llllll2 < 0.0 && saturate(llllll14) < 0.0001)
                    {
                        l10 = llllllllllll11;
                        lllllllllll11 = 1 - lllllllllll11;
                        float lllllllllllllllllllllllllllllll17 = lllllllllll11 * llllllllll11;
                        float l18 = smoothstep(-lllllllllllllllllllllllllllllll17 + 0.01, -llllllllll11, llllll2);
                        float3 ll18 = lerp(llllllllllll11.rgb, lllllllllllllll13.rgb, 1 - llllllllllll11.a);
                        l10 = float4(lerp(llllllllllllllllllllllllll16, ll18, l18), lllllllllllllll13.a);
                    }
                }
                if (lllllll11 == 0 && lllllllllllllllllllllllllllllll10 == 0 && lllllllllllll11 == 1)
                {
                    l10 = float4(lerp(llllllllllllllllllllllllll16, l10.rgb, saturate(lllllllllll4)), lllllllllllllll13.a);
                }
            }
        }
        #if _URP
        if (_LightSource != 1) 
        #endif
        {
#if _ENABLE_SPECULAR || !_USE_OPTIMIZATION_DEFINES
            if (lllllllllllllllllll11 == 1)
            {
#if _URP
#else
                lllllll2 = CalculateSpecularMask(llllllllllll2, llllllllllll13.dir, lll2, llllllllllllllllllllll11, lllllllllllllllllllllll11, llllll2);
                lllllll2 *= llllllllllllllllllllllll11;
                if (lllllllllllll11 == 1)
                {
                    lllllll2 *= lllllllllll4;
                }
#endif
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SPECULAR_BLENDING
            llllllllllllllllllll11 = _SPECULAR_BLENDING;
#endif
#endif
                half4 lll18;
                {
                    lll18 = lllllllllllllllllllll11;
                }
                DoBlending(l10, lllllll2, llllllllllllllllllll11, lll18);
            }
#endif
        }
#if _URP
    l10 += half4(surface.emission, 0);
#else
        l10 += half4(o.Emission, 0);
#endif
    }
    else 
    {
        ToonShadingData toonShadingData;
        toonShadingData.enableToonShading = llllllllllllllllllllllllllllll10;
#if _URP
        toonShadingData.normalWS = inputData.normalWS;
#endif
        toonShadingData.normalWSNoMap = lllllllllll13;
        toonShadingData.cellTransitionSmoothness = llllllllllllllllllllll10;
        toonShadingData.numberOfCells = llllllllllllllll13;
        toonShadingData.specularEdgeSmoothness = lllllllllllllllllllllll11;
        toonShadingData.shadingAffectByNormalMap = llllllllllllllllll11;
        toonShadingData.specularAffectedByNormalMap = lllllllllllllllllllllllll11;
#if _USE_OPTIMIZATION_DEFINES
#if _ENABLE_TOON_SHADING 
                toonShadingData.enableToonShading = 1;
#else
                toonShadingData.enableToonShading = 0;
#endif
#endif
#if _SHADING_BLINNPHONG       
        if (lllllllllllllllllllllllllllll10 == 0) 
        {
#if _URP
#if UNITY_VERSION >= 202120
            l10 = UniversalFragmentBlinnPhong(inputData, surface.albedo, half4(surface.specular, surface.smoothness), surface.smoothness, surface.emission, surface.alpha,lllllllllllllll10, toonShadingData);
#else
            l10 = UniversalFragmentBlinnPhong(inputData, surface.albedo, half4(surface.specular, surface.smoothness), surface.smoothness, surface.emission, surface.alpha, toonShadingData);
#endif
#else
#endif
        }
#endif        
#if _SHADING_PBR
        if (lllllllllllllllllllllllllllll10 == 1) 
        {      
#if _URP
            l10 = UniversalFragmentPBR(inputData, surface, toonShadingData);
#else
#if !_PASSFORWARDADD
#if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
#else
        LightingStandard_GI_Toon(o, giInput, gi, toonShadingData);
#if defined(_OVERRIDE_BAKEDGI)
            gi.indirect.diffuse = l.DiffuseGI;
            gi.indirect.specular = l.SpecularGI;
#endif
        l10 = LightingStandard_Toon (o, d.worldSpaceViewDir, gi, toonShadingData);
        l10 += half4(o.Emission, 0);
#endif     
#else
#if _USESPECULAR
#elif _BDRF3 || _SIMPLELIT
#else
                  l10 = LightingStandard_Toon (o, d.worldSpaceViewDir, gi, toonShadingData);
#endif
#endif
#endif
        }
#endif
    }
    float llllllllllll4 = 0;
    float lllll18 = 0;
    if (llllllllllllllllllllllllllllll10 == 1)
    {
#if _URP
        Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
        float llllll18 = dot(mainLight.direction, lll4);
        float lllllll18 = mainLight.shadowAttenuation;
#else
        float llllll2 = dot(llllllllllll13.dir, lll4);
#endif
#if _ENABLE_RIM || !_USE_OPTIMIZATION_DEFINES
#if !_USE_OPTIMIZATION_DEFINES
        if (lllllllllllllllllllllllllll11 == 1)
#endif
        {
#if _URP         
            llllllllllll4 = CalculateRimMask(lll4, lll2, llllllllllllllllllllllllllllll11, lllllllllllllllllllllllllllllll11, llllll18, ll12, lllllllllllll11, lllllll11, lllllll18);
#else
            llllllllllll4 = CalculateRimMask(lll4, lll2, llllllllllllllllllllllllllllll11, lllllllllllllllllllllllllllllll11, llllll2, ll12, lllllllllllll11, lllllll11, lllllllllll4);
#endif   
            llllllllllll4 *= l12;
#if _USE_OPTIMIZATION_DEFINES
#ifdef _RIM_BLENDING
                        llllllllllllllllllllllllllll11 = _RIM_BLENDING;
#endif
#endif   
            llllllllllll4 = saturate(llllllllllll4);
            DoBlending(l10, llllllllllll4, llllllllllllllllllllllllllll11, lllllllllllllllllllllllllllll11);
        }
#endif
    }
#if _ENABLE_STYLING || !_USE_OPTIMIZATION_DEFINES   
#if !_USE_OPTIMIZATION_DEFINES
    if (llll12 == 1)
#endif
    {
#ifdef _EMISSION 
#if _URP
        float3 lllllllll18 = surface.emission;
#else
        float3 lllllllll18 = o.Emission;
#endif
        float llllllllllll18 = max(max(lllllllll18.r, lllllllll18.g), lllllllll18.b);
#endif
#if !_URP
        if (lllllllllllllllllllll12 == 1)
        {
            if (lllllllllllllllllll11 == 0 || llllllllllllllllllllll12 == 0) 
            {
                float lllllllllllll18 = saturate(llllll2);
                llll14 = CalculateSpecularMask(lllllllllllllllllllll13, llllllllllll13.dir, lll2, lllllllllllllllllllllll12, llllllllllllllllllllllll12, lllllllllllll18);
                llll14 = saturate(llll14);
                llll14 *= lllllllllllllllllllll16;
            }
            else
            {
                llll14 = saturate(lllllll2);
            }
        }
#endif
        if (llllllllllllllllllllllllllllll12 == 1)
        {
            if (lllllllllllllllllllllllllll11 == 1 && lllllllllllllllllllllllllllllll12 == 1)
            {
                lllll18 = llllllllllll4;
            }
            else
            {
#if _URP
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                float llllllllllllll18 = dot(mainLight.direction, lllllllllllllllllllll13);
                lllll18 = CalculateRimMask(lllllllllllllllllllll13, lll2, l13, ll13, llllllllllllll18, lll13, lllllllllllll11, lllllll11, mainLight.shadowAttenuation);
#else
                lllll18 = CalculateRimMask(lllllllllllllllllllll13, lll2, l13, ll13, llllll2, lll13, lllllllllllll11, lllllll11, lllllllllll4);
#endif
            }
            lllll18 = saturate(lllll18);
        }
        if (lllllllllllllllllllll12 == 1 && lllllllllllllllllllllllll12 != 0)
        {
            lllllllllllllllllllll16 = 1 - ((1 - lllllllllllllllllllll16) - llll14);
        }
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SHADING_STYLING_DRAWSPACE
        uvSpaceDataShading.drawSpace = _SHADING_STYLING_DRAWSPACE;
#endif
#ifdef _SHADING_STYLING_COORDINATESYSTEM
        uvSpaceDataShading.coordinateSystem = _SHADING_STYLING_COORDINATESYSTEM;
#endif
#endif
#if _URP
        float2 lllllllllllllll18 = ConvertToDrawSpace(inputData, lllllllll1, uvSpaceDataShading, llllllllllllllllllllllllllll0, uvSets);
#else
        float2 lllllllllllllll18 = ConvertToDrawSpace(d.worldSpacePosition, d.worldSpaceNormal, lllllllll1, uvSpaceDataShading, llllllllllllllllllllllllllll0, uvSets);
#endif
        lllllllllllllll18 = PixelateDrawSpaceUV(lllllllllllllll18, uvSpaceDataShading, lllllllllllll12, llllllllllllll12);
        half llllllllllllllllll18 = lllllllllllll12 == 1 ? 0 : lllllllllll8;
        float lllllllllllllllllll18 = stylingDataShading.density;
        float llllllllllllllllllllllllll5 = stylingDataShading.size;
        float lllllllllllllllllllll18 = 1;
#if _ENABLE_SHADING_STYLING || !_USE_OPTIMIZATION_DEFINES   
#if !_USE_OPTIMIZATION_DEFINES
        if (lllllll12 != 0)
#endif        
        {
            float llllllllllllllllllllll18 = 0;
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SHADING_STYLING_BLENDING
                    positionAndBlendingDataShading.blending = _SHADING_STYLING_BLENDING;
#endif                   
#ifdef _SHADING_STYLE
                stylingDataShading.style = _SHADING_STYLE;
#endif
#if _SHADING_STYLING_RANDOMIZER
                stylingRandomDataShading.enableRandomizer = 1;
#else
                stylingRandomDataShading.enableRandomizer = 0;
#endif
#endif
            RequiredNoiseData requiredNoiseDataShading;
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SHADING_STYLING_RANDOMIZER_PERLIN
            requiredNoiseDataShading.perlinNoise = 1;
#else
            requiredNoiseDataShading.perlinNoise = 0;
#endif
#ifdef _SHADING_STYLING_RANDOMIZER_PERLIN_FLOORED
            requiredNoiseDataShading.perlinNoiseFloored = 1;
#else
            requiredNoiseDataShading.perlinNoiseFloored = 0;
#endif         
#ifdef _SHADING_STYLING_RANDOMIZER_WHITE
            requiredNoiseDataShading.whiteNoise = 1;
#else
            requiredNoiseDataShading.whiteNoise = 0;
#endif
#ifdef _SHADING_STYLING_RANDOMIZER_WHITE_FLOORED
            requiredNoiseDataShading.whiteNoiseFloored = 1;
#else
            requiredNoiseDataShading.whiteNoiseFloored = 0;
#endif            
#else            
            requiredNoiseDataShading.perlinNoise = 1;
            requiredNoiseDataShading.perlinNoiseFloored = 1;
            requiredNoiseDataShading.whiteNoise = 1;
            requiredNoiseDataShading.whiteNoiseFloored = 1;
#endif
            float lllllllllllllllllllllll18 = (lllllllllllllllllllllllllllll13);
            if (lllllllllllllll12 == 1 && llllllllllllllll12 == 1
#if _URP
                && _LightSource != 1
#endif                
                )
            {
                stylingDataShading.opacityFalloff *= l14;
                stylingDataShading.sizeFalloff *= l14;
            }
            if (positionAndBlendingDataShading.isInverted == 1)
            {
                lllllllllllllllllllllll18 = 1 - saturate(lllllllllllllllllllllll18);
            }
            float llllllllllllllllllllllll18 = 0;
            float lllllllllllllllllllllllll18 = 0;
            float llllllllllllllllllllllllll18 = 0;
            float lllllllllllllllllllllllllll18 = 0;
            bool llllllllllllllllllllllllllll18 = lllllllllllllllllllll12 == 1 && lllllllllllllllllllllllll12 != 0;
            bool lllllllllllllllllllllllllllll18 = llllllllllllllllllllllllllllll12 == 1 && llll13 != 0;
            bool llllllllllllllllllllllllllllll18 = false;          
        #ifdef _EMISSION
            llllllllllllllllllllllllllllll18 = true;
        #endif
            bool lllllllllllllllllllllllllllllll18 = llllllllllllllllllllllllllll18 || lllllllllllllllllllllllllllll18 || llllllllllllllllllllllllllllll18;
            if (lllllllllllllllllllllllllllllll18)
            {
                float l19 = max(saturate(stylingDataShading.sizeFalloff), 0.0001);
            #ifdef _EMISSION
                lllllllllllllllllllllllllll18 = smoothstep(0, l19, llllllllllll18);
            #endif
                if (llllllllllllllllllllllllllll18)
                {
#           ifndef _URP2D
                    lllllllllllllllllllllllll18 = smoothstep(0, l19, llll14);
            #else
                    lllllllllllllllllllllllll18 = smoothstep(0, l19, lllllll2);
            #endif
                    lllllllllllllllllllllllllll18 = max(lllllllllllllllllllllllllll18, lllllllllllllllllllllllll18);
                }
                if (lllllllllllllllllllllllllllll18)
                {
                    llllllllllllllllllllllllll18 = smoothstep(0, l19, lllll18);
                    lllllllllllllllllllllllllll18 = max(lllllllllllllllllllllllllll18, llllllllllllllllllllllllll18);
                }
                lllllllllllllllllllllllllll18 = saturate(lllllllllllllllllllllllllll18);
                stylingDataShading.size -= lllllllllllllllllllllllllll18;
                stylingDataShading.size = saturate(stylingDataShading.size);
            }
            float ll19 = min(saturate(stylingDataShading.sizeMin), saturate(stylingDataShading.size));
            if (stylingDataShading.sizeMinFromControlMap == 1)
            {
                float lll19 = (1 - saturate(tex2D(_StylingShadingControlMap, lllllllll1).r)) * saturate(_StylingShadingControlMapStrength);
                ll19 = max(ll19, lll19);
            }
            if (lllllllllllllllllllllllllllllll18)
            {
                ll19 = saturate(ll19 - lllllllllllllllllllllllllll18);
            }
            stylingDataShading.sizeMin = ll19;
            if (stylingDataShading.style == 0) 
            {
                float lllllllllllllllllll18 = stylingDataShading.density;
                float llllllllllllllllllllllllll5 = stylingDataShading.size;
                float llllll19 = ll19 / 2;
                llllllllllllllllllllllllll5 = stylingDataShading.size / 2;
                if (lllllllll12 == 0)
                {
                    llllllllllllllll13 = llllllllll12;
                }
                else
                {
                    llllllllllllllll13 = lllllllllllllllllllll10;
                }
#if _USE_OPTIMIZATION_DEFINES            
#ifdef _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING
                        llllllllllllllll13 = _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING;
#endif                            
#endif
                float lllllll19 = (1. / llllllllllllllll13) * llllllllllll12;
                int llllllll19 = ceil((max(lllllllllllllllllllllll18 - lllllll19, 0)) * llllllllllllllll13);
                llllllll19 = llllllllllllllll13 - llllllll19;
                int lllllllll19 = llllllll19;
                if (llllll19 > 0.0001)
                {
                    llllllll19 = max(llllllll19, 1);
                }
                float llllllllll19 = stylingDataShading.rotation;
                float lllllllllll19 = radians(llllllllll19);
                float llllllllllll19 = stylingDataShading.rotationBetweenCells;
                float lllllllllllll19 = radians(llllllllllll19);
                float2 llllllllllllll19; 
                NoiseSampleData noiseSampleData; 
                lllllllllllllllllllll18 = 1;
                float lllllllllllllllll1 = 0;
    #if _USE_OPTIMIZATION_DEFINES            
                [unroll(llllllllllllllll13)]
    #else
                [unroll(15)]
#endif
                for (int i = 1; i <= llllllll19; i++)
                {
                    llllllllllllllllllllllllll5 = stylingDataShading.size / 2;
                    float llllllllllllllll19 = i - 1;
                    float llllllllll5 = lllllllllll19 + lllllllllllll19 * llllllllllllllll19;
                    lllllllllllllll18 += lllllllllllllllll1; 
                    llllllllllllll19 = RotateUVRadians(lllllllllllllll18, llllllllll5);
                    noiseSampleData = SampleNoiseData(llllllllllllll19, stylingDataShading, stylingRandomDataShading, requiredNoiseDataShading, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
                    lllllllllllllllll1 += (float) stylingDataShading.density;
                    float llllllllllllllllll19 = llllllllllllll19.x;
                    float lllllllllllllllllll19 = llllllllllllll19.y; 
                    float llllllllllllllll8 = max(stylingDataShading.dashDensity, 0.0001);
                    llllllllllllllllll19 *= stylingDataShading.density;
                    lllllllllllllllllll19 *= llllllllllllllll8; 
                    float lllllllllllllllllllll19 = floor(llllllllllllllllll19); 
                    float llllllllllllllllllllll19 = lllllllllllllllllll19; 
                    float lllllllllllllllllllllll19 = llllllllllllllllll18 ? fwidth(llllllllllllllllll19) : 0.0;
                    float llllllllllllllllllll8 = 0;
                    float llllllllllllllllllllll8 = 0;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        llllllllllllllllll19 += noiseSampleData.perlinNoise * stylingRandomDataShading.noiseIntensity;
                        lllllllllllllllllll19 += noiseSampleData.perlinNoise * stylingRandomDataShading.noiseIntensity; 
                        llllllllllllllllllllll19 = lllllllllllllllllll19; 
                        lllllllllllllllllllllll19 = llllllllllllllllll18 ? fwidth(llllllllllllllllll19) : 0.0;
                        if (stylingRandomDataShading.thicknessRandomMode == 0)
                        {
                            llllllllllllllllllll8 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.thicknessRandomMode == 1) 
                        {
                            llllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored;
                        }
                        else 
                        {
                            llllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
                        }
                        llllllllllllllllllll8 *= stylingRandomDataShading.thicknesshRandomIntensity;
                        if (stylingRandomDataShading.spacingRandomMode == 0)
                        {
                            llllllllllllllllllllll8 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.spacingRandomMode == 1) 
                        {
                            llllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored;
                        }
                        else 
                        {
                            llllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
                        }
                    }
                    float llllllllllllllllllllllllll19 = (float) (llllllllllllllll13 - i) / llllllllllllllll13;
                    float lllllllllllllllllllllllllll19 = remap(0, 1, 0, lllllll19, llllllllllll12);
                    float llllllllllllllllll5;
                    float lllllllllllllllllllllllllll8;
                    float llllllllllllllllllllllllllllll19 = 0;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float llllllllllllllllllllllllll8 = 0;
                        if (stylingRandomDataShading.lengthRandomMode == 0)
                        {
                            llllllllllllllllllllllllll8 = noiseSampleData.whiteNoise * saturate(1 - stylingRandomDataShading.noiseIntensity);
                        }
                        else if (stylingRandomDataShading.lengthRandomMode == 1)
                        {
                            llllllllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored; 
                        }
                        else
                        {
                            llllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomDataShading.noiseIntensity))) / 2); 
                        }
                        lllllllllllllllllllllllllll8 = llllllllllllllllllllllllll8 * stylingRandomDataShading.lengthRandomIntensity;
                        llllllllllllllllllllllllllllll19 = remap(0, 1, 0, llllllllllllllllllllllllll19 + lllllllllllllllllllllllllll19, lllllllllllllllllllllllllll8);
                    }
                    float l20 = llllllllllllllllllllllllll19 + lllllllllllllllllllllllllll19 - llllllllllllllllllllllllllllll19;
                    bool ll20 = i == 1 && llllll19 > 0.0001;
                    bool lll20 = llllllllllllllll13 == 1 && ll20 && abs(l20) <= 0.00001;
                    llllllllllllllllll5 = lll20 ? 1.0 : remap(0, l20, 0, 1, lllllllllllllllllllllll18);
                    if (!lll20 && i == llllllllllllllll13 && sign(lllllllllllllllllllllll18) == 1)
                    {
                        float llllllllllllllllllllllllllllll19 = 0;
                        if (stylingRandomDataShading.enableRandomizer == 1)
                        {
                            llllllllllllllllllllllllllllll19 = remap(0, 1, 0, 1 - lllllll19, lllllllllllllllllllllllllll8);
                        }
                        llllllllllllllllll5 = remap(0, lllllll19, 1 - lllllll19 + llllllllllllllllllllllllllllll19, 1 + llllllllllllllllllllllllllllll19, lllllllllllllllllllllll18);
                    }
                    if (i == llllllllllllllll13 && sign(lllllllllllllllllllllll18) == -1)
                    {
                        float lllll20 = (float) 1. / llllllllllllllll13;
                        lllllllllllllllllllllllllll19 = remap(0, 1, 0, lllll20, llllllllllll12);
                        float llllllllllllllllllllllllllllll19 = 0;
                        if (stylingRandomDataShading.enableRandomizer == 1)
                        {
                            llllllllllllllllllllllllllllll19 = remap(0, 1, 0, 1 - lllllllllllllllllllllllllll19, lllllllllllllllllllllllllll8);
                        }
                        llllllllllllllllll5 = remap(0, -1, 1 - lllllllllllllllllllllllllll19 + llllllllllllllllllllllllllllll19, 0, lllllllllllllllllllllll18);
                    }
                    float llllllllllllllllllllllllllll8 = smoothstep(1 - stylingDataShading.sizeFalloff, 1, llllllllllllllllll5);
                    if (lllllllllllllllllllll16 <= 0 && lllllllllllllllllllllll18 > 0)
                    {
                    }
                    float llllllll20 = ll20 ? llllll19 : 0;
                    bool lllllllll20 = i > lllllllll19;
                    float llllllllll20 = lllllllll20 ? 0 : max(llllllllllllllllllllllllll5 - llllllllllllllllllllllllllll8, 0);
                    llllllllllllllllllllllllllll8 = max(llllllllll20, llllllll20);
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float lllllllllllllllllllll8 = remap(0, 1, 0.0, llllllllllllllllllllllllllll8, llllllllllllllllllll8);
                        llllllllllllllllllllllllllll8 = saturate(llllllllllllllllllllllllllll8 - lllllllllllllllllllll8);
                        float lllllllllllllllllllllll8 = llllllllllllllllllllllllllll8;
                        if (stylingDataShading.dashEnabled == 1 && _StylingShadingDashesType == 1)
                        {
                            lllllllllllllllllllllll8 = CalculateHatchingDashSafeSpacingHalfWidth(
                                llllllllllllllllllllllllllll8,
                                stylingDataShading.hardness,
                                lllllllllllllllllllllll19,
                                llllllllllllllllll18
                            );
                        }
                        float llllllllllllllllllllllll8 = remap(0, 1, -0.5 + lllllllllllllllllllllll8, 0.5 - lllllllllllllllllllllll8, llllllllllllllllllllll8);
                        llllllllllllllllll19 += llllllllllllllllllllllll8 * stylingRandomDataShading.spacingRandomIntensity * saturate(1 - stylingRandomDataShading.noiseIntensity); 
                    }
                    llllllllllllllllll19 = abs(frac(llllllllllllllllll19) - 0.5);
                    float llllllllllllll20;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float llllllllllllllllllllllllllllll8 = 0;
                        if (stylingRandomDataShading.hardnessRandomMode == 0) 
                        {
                            llllllllllllllllllllllllllllll8 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.hardnessRandomMode == 1) 
                        {
                            llllllllllllllllllllllllllllll8 = noiseSampleData.perlinNoiseFloored * 5;
                        }
                        else
                        {
                            llllllllllllllllllllllllllllll8 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
                        }
                        llllllllllllll20 = remap(0, 1, 0, llllllllllllllllllllllllllll8, min(saturate(stylingDataShading.hardness - llllllllllllllllllllllllllllll8 * stylingRandomDataShading.hardnessRandomIntensity), stylingDataShading.hardness));
                    }
                    else
                    {
                        llllllllllllll20 = remap(0, 1, 0, llllllllllllllllllllllllllll8, stylingDataShading.hardness);
                    }
                    float llllllllllllllll20 = (llllllllllllllllllllllllllll8 > 0.0001) ? saturate(llllllllllllll20 / max(llllllllllllllllllllllllllll8, 0.0001)) : stylingDataShading.hardness;
                    float lllllllllllllllllll6 = llllllllllllllllll19;
                    float llllllllllllllllll6 = CalculateHatching1DMaskFromDistance(
                            lllllllllllllllllll6,
                            llllllllllllllllllllllllllll8,
                            llllllllllllllll20,
                            llllllllllllllllllllllllll5,
                            stylingDataShading.size,
                            lllllllllllllllllllllll19,
                            llllllllllllllllll18
                    );
                    llllllllllllllllll19 = llllllllllllllllll6;
                    if (stylingDataShading.dashEnabled == 1 && i == 1)
                    {
                        float l9 = CalculateHatchingDashContinuity(
                            stylingDataShading.dashTransitionPosition,
                            stylingDataShading.dashTransitionSoftness,
                            llllllllllllllllll5
                        );
                        float ll9 = saturate(stylingDataShading.dashLength) * 0.5;
                        float llllllllllllllllllllll6 = lerp(ll9, 0.5, l9);
                        lllllllllllllllllll19 += lllllllllllllllllllll19 * saturate(stylingDataShading.dashOffset);
                        float llllllllllllllllllllll20 = llllllllllllllllll18 ? fwidth(llllllllllllllllllllll19) : 0.0;
                        float llllllllllllllllllll6 = abs(frac(lllllllllllllllllll19) - 0.5);
                        llllllllllllllllll19 = ApplyHatchingDashMode(
                                llllllllllllllllll6,
                                lllllllllllllllllll6,
                                llllllllllllllllllll6,
                                llllllllllllllllllllllllllll8,
                                llllllllllllllllllllll6,
                                llllllllllllllll20,
                                lllllllllllllllllllllll19,
                                llllllllllllllllllllll20,
                                stylingDataShading.dashType,
                                stylingDataShading.dashRoundness,
                                llllllllllllllllll18
                            );
                    }
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float llllll9;
                        if (stylingRandomDataShading.opacityRandomMode == 0) 
                        {
                            llllll9 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.opacityRandomMode == 1) 
                        {
                            llllll9 = noiseSampleData.perlinNoiseFloored * 5;
                        }
                        else 
                        {
                            llllll9 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
                        }
                        llllllllllllllllll19 = saturate(llllllllllllllllll19 - (llllll9 * stylingRandomDataShading.opacityRandomIntensity));
                    }
                    float lllllll9 = (lllllllll20 || ll20)? 0: smoothstep(saturate(min(1 - stylingDataShading.opacityFalloff, 1)), 1, llllllllllllllllll5);
                    llllllllllllllllll19 *= 1 - lllllll9;
                    llllllllllllllllll19 = 1 - llllllllllllllllll19;
                    lllllllllllllllllllll18 = min(llllllllllllllllll19, lllllllllllllllllllll18);
                }
                lllllllllllllllllllll18 = 1 - lllllllllllllllllllll18;
                lllllllllllllllllllll18 *= stylingDataShading.opacity;
                lllllllllllllllllllll18 = 1 - lllllllllllllllllllll18;
                llllllllllllllllllllll18 = lllllllllllllllllllll18;
            }
            else if (stylingDataShading.style == 1) 
            {
                float2 llllllllllll9 = lllllllllllllll18;
                float2 lllllll5 = RotateUV(llllllllllll9, stylingDataShading.rotation);
                NoiseSampleData noiseSampleData = SampleNoiseData(lllllll5, stylingDataShading, stylingRandomDataShading, requiredNoiseDataShading, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
                if (false)
                {
                } 
                float llllllllllllllllllllllllllll20 = 1 - lllllllllllllllllllllll18;
                float lllllllllllllllllllllllllllll9 = Halftones(llllllllllllllllllllllllllll20, lllllll5, stylingDataShading, stylingRandomDataShading, noiseSampleData, llllllllllllllllll18);
                llllllllllllllllllllll18 = lllllllllllllllllllllllllllll9;
            }
            if (false)
            {
            }
#if _USE_OPTIMIZATION_DEFINES
#if _ENABLE_STYLING_DISTANCEFADE
                     generalStylingData.enableDistanceFade = 1;
#else
                    generalStylingData.enableDistanceFade = 0;
#endif
#endif
            if (generalStylingData.enableDistanceFade == 1)
            {
                float llllllllllllllllllllllllllllll20 = lllllllllllllllllllllll18;
                if (stylingDataShading.style == 0)
                {
                    int llllllllllllllll13;
                    if (lllllllll12 == 0)
                    {
                        llllllllllllllll13 = llllllllll12;
                    }
                    else
                    {
                        llllllllllllllll13 = lllllllllllllllllllll10;
                    }
                    float lllllll19 = (1. / llllllllllllllll13) * llllllllllll12;
                    float lllllllllllllllllllllllllll19 = remap(0, 1, 0, lllllll19, llllllllllll12);
                    llllllllllllllllllllllllllllll20 -= -1 + ((llllllllllllllll13 - 1.) / llllllllllllllll13) + lllllllllllllllllllllllllll19;
                }
                float lll21 = distance(_WorldSpaceCameraPos, d.worldSpacePosition);
                float llll21 = max(llllllllllllllllllllllllllllll20, 1 - stylingDataShading.opacityFalloff);
                llll21 = remap(1 - stylingDataShading.opacityFalloff, 1, 0, 1, llll21);
                float lllll21 = max(llllllllllllllllllllllllllllll20, 1 - stylingDataShading.sizeFalloff);
                lllll21 = remap(1 - stylingDataShading.sizeFalloff, 1, 0, 1, lllll21);
                float llllll21 = lerp(0.0, 1, saturate(1 - stylingDataShading.size)); 
                if (generalStylingData.adjustDistanceFadeValue == 1)
                {
                    llllll21 = generalStylingData.distanceFadeValue;
                }
                lllll21 = max(llllll21, lllll21 * 2);
                llll21 = max(llllll21, llll21);
                float lllllll21 = max(lllll21, llll21);
                lllllll21 = saturate(lllllll21);
                llllllllllllllllllllll18 = lerp(llllllllllllllllllllll18, lllllll21, saturate(((lll21 - generalStylingData.distanceFadeStartDistance) / generalStylingData.distanceFadeFalloff)));
            }
            if (positionAndBlendingDataShading.isInverted == 1)
            {
                llllllllllllllllllllll18 = 1 - llllllllllllllllllllll18;
            }
            DoBlending(l10, 1 - llllllllllllllllllllll18, positionAndBlendingDataShading.blending, stylingDataShading.color);
            if (false)
            {
            }
            if (false)
            {
            }
        }
#endif
    #if _URP
        if (_LightSource != 1) 
    #endif
        {
#if (_ENABLE_CASTSHADOWS_STYLING && _STYLING_CASTSHADOWS_SYNC_WITH_OTHER_STYLING != 1) || !_USE_OPTIMIZATION_DEFINES   
#if !_USE_OPTIMIZATION_DEFINES
            if (lllllllllllllll12 && llllllllllllllll12 != 1)   
#endif
            {
#if _USE_OPTIMIZATION_DEFINES
#ifdef _CASTSHADOWS_STYLING_BLENDING
                positionAndBlendingDataCastShadows.blending = _CASTSHADOWS_STYLING_BLENDING;
#endif
#ifdef _CASTSHADOWS_STYLING_DRAWSPACE
                uvSpaceDataCastShadows.drawSpace = _CASTSHADOWS_STYLING_DRAWSPACE;
#endif
#ifdef _CASTSHADOWS_STYLING_COORDINATESYSTEM
                uvSpaceDataCastShadows.coordinateSystem = _CASTSHADOWS_STYLING_COORDINATESYSTEM;
#endif            
#ifdef _CASTSHADOWS_STYLE
                stylingDataCastShadows.style = _CASTSHADOWS_STYLE;
#endif
#if _CASTSHADOWS_STYLING_RANDOMIZER
                stylingRandomDataCastShadows.enableRandomizer = 1;
#else
                stylingRandomDataCastShadows.enableRandomizer = 0;
#endif
#endif
                RequiredNoiseData requiredNoiseDataCastShadows;
#if _USE_OPTIMIZATION_DEFINES
#ifdef _CASTSHADOWS_STYLING_RANDOMIZER_PERLIN
                requiredNoiseDataCastShadows.perlinNoise = 1;
#else
                requiredNoiseDataCastShadows.perlinNoise = 0;
#endif
#ifdef _CASTSHADOWS_STYLING_RANDOMIZER_PERLIN_FLOORED
                requiredNoiseDataCastShadows.perlinNoiseFloored = 1;
#else
                requiredNoiseDataCastShadows.perlinNoiseFloored = 0;
#endif         
#ifdef _CASTSHADOWS_STYLING_RANDOMIZER_WHITE
                requiredNoiseDataCastShadows.whiteNoise = 1;
#else
                requiredNoiseDataCastShadows.whiteNoise = 0;
#endif
#ifdef _CASTSHADOWS_STYLING_RANDOMIZER_WHITE_FLOORED
                requiredNoiseDataCastShadows.whiteNoiseFloored = 1;
#else
                requiredNoiseDataCastShadows.whiteNoiseFloored = 0;
#endif            
#else            
                requiredNoiseDataCastShadows.perlinNoise = 1;
                requiredNoiseDataCastShadows.perlinNoiseFloored = 1;
                requiredNoiseDataCastShadows.whiteNoise = 1;
                requiredNoiseDataCastShadows.whiteNoiseFloored = 1;
#endif
#if _URP
            float2 llllllll21 = ConvertToDrawSpace(inputData, lllllllll1, uvSpaceDataCastShadows, llllllllllllllllllllllllllll0, uvSets);
#else
                float2 llllllll21 = ConvertToDrawSpace(d.worldSpacePosition, d.worldSpaceNormal, lllllllll1, uvSpaceDataCastShadows, llllllllllllllllllllllllllll0, uvSets);
#endif
            llllllll21 = PixelateDrawSpaceUV(llllllll21, uvSpaceDataCastShadows, lllllllllllllllllll12, llllllllllllllllllll12);
            half llllllllll21 = lllllllllllllllllll12 == 1 ? 0 : lllllllllll8;
#ifdef _EMISSION
            stylingDataCastShadows.size = saturate(stylingDataCastShadows.size - llllllllllll18);
#endif
                lllllllllllllllllllll16 = l14;
                float llllllllllllllllllllll18 = 0;
                if (stylingDataCastShadows.style == 0) 
                {
                    float llllllllllll21 = stylingDataCastShadows.rotation;
                    float lllllllllllll21 = radians(llllllllllll21);
                    float llllllllllllll21 = stylingDataCastShadows.rotationBetweenCells;
                    float lllllllllllllll21 = radians(llllllllllllll21);
                    float llllllllllllllll21 = llllllllllllllllll12;
                    llllllllllllllll21 = min(llllllllllllllll21, 0.99);
                    float lllllllllllllllll21 = 1;
                    float llllllllllllllll13 = lllllllllllllllll12;
            #if _USE_OPTIMIZATION_DEFINES            
                #ifdef _CASTSHADOWS_STYLING_NUMBER_OF_CELLS_HATCHING
                        llllllllllllllll13 = _CASTSHADOWS_STYLING_NUMBER_OF_CELLS_HATCHING;
                #endif                           
                [unroll(llllllllllllllll13)]
            #else
                [unroll(15)]
#endif
                    for (int j = 1; j <= llllllllllllllll13; j++)
                    {
                        l14 = min(j / llllllllllllllll13, lllllllllllllllllllll16);
                        if (llllllllllllllll13 != 1)
                        {
                            float lllllllllll10 = 0;
                            if (llllllllllllllll13 <= 1)
                            {
                                lllllllllll10 = 0.0;
                            }
                            else
                            {
                                float llllllllllllllllllll21 = (float) j - 1;
                                float lllllllllllllllllllll21 = (float) (llllllllllllllll13 - 1);
                                float llllllllllllllllllllll21 = llllllllllllllllllll21 / lllllllllllllllllllll21;
                                lllllllllll10 = lerp(1.0, llllllllllllllllllllll21, llllllllllllllll21);
                            }
                            float lllllllllllllllllllllll21 = min(lllllllllll10, lllllllllllllllllllll16); 
                            lllllllllllllllllllllll21 = remap(0, lllllllllll10, 0, 1, lllllllllllllllllllll16);
                            l14 = lllllllllllllllllllllll21;
                            l14 = max(l14, lllllllllllllllllllll16);
                        }
                        else
                        {
                            l14 = lllllllllllllllllllll16;
                        }
                        float llllllllllllllll19 = j - 1;
                        float llllllllll5 = lllllllllllll21 + lllllllllllllll21 * llllllllllllllll19;
                        float2 llllllllllllll19 = RotateUVRadians(llllllll21, llllllllll5);
                        llllllllllllll19.x += (j - 1) / (float) llllllllllllllll13 * stylingDataCastShadows.density; 
                        NoiseSampleData noiseSampleData = SampleNoiseData(llllllllllllll19, stylingDataCastShadows, stylingRandomDataCastShadows, requiredNoiseDataCastShadows, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
                        float lllllllllllllllllllllllllll21 = Hatching(1 - l14, llllllllllllll19, stylingDataCastShadows, stylingRandomDataCastShadows, noiseSampleData, llllllllll21);
                        lllllllllllllllllllllllllll21 = 1 - lllllllllllllllllllllllllll21;
                    {
                            lllllllllllllllll21 = min(lllllllllllllllllllllllllll21, lllllllllllllllll21);
                        }
                    }
                    llllllllllllllllllllll18 = lllllllllllllllll21;
                }
                else if (stylingDataCastShadows.style == 1) 
                {
                    float2 lllllll5 = RotateUV(llllllll21, stylingDataCastShadows.rotation);
                    NoiseSampleData noiseSampleData = SampleNoiseData(lllllll5, stylingDataCastShadows, stylingRandomDataCastShadows, requiredNoiseDataCastShadows, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
                    float lllllllllllllllllllllllllllll9 = Halftones(1 - l14, lllllll5, stylingDataCastShadows, stylingRandomDataCastShadows, noiseSampleData, llllllllll21);
                    llllllllllllllllllllll18 = lllllllllllllllllllllllllllll9;
                }
                DoBlending(l10, 1 - llllllllllllllllllllll18, positionAndBlendingDataCastShadows.blending, stylingDataCastShadows.color);
            }
#endif        
        }
        #if _URP
        if (_LightSource != 1) 
        #endif
        {
#if _ENABLE_SPECULAR_STYLING || !_USE_OPTIMIZATION_DEFINES   
#if !_USE_OPTIMIZATION_DEFINES
            if (lllllllllllllllllllll12 && lllllllllllllllllllllllll12 != 1)
#else
            if (lllllllllllllllllllllllll12 != 1)
#endif
            {
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SPECULAR_STYLING_BLENDING
                positionAndBlendingDataSpecular.blending = _SPECULAR_STYLING_BLENDING;
#endif
#ifdef _SPECULAR_STYLING_DRAWSPACE
                uvSpaceDataSpecular.drawSpace = _SPECULAR_STYLING_DRAWSPACE;
#endif
#ifdef _SPECULAR_STYLING_COORDINATESYSTEM
                uvSpaceDataSpecular.coordinateSystem = _SPECULAR_STYLING_COORDINATESYSTEM;
#endif            
#ifdef _SPECULAR_STYLE
                stylingDataSpecular.style = _SPECULAR_STYLE;
#endif
#if _SPECULAR_STYLING_RANDOMIZER
                stylingRandomDataSpecular.enableRandomizer = 1;
#else
                stylingRandomDataSpecular.enableRandomizer = 0;
#endif
#endif
                RequiredNoiseData requiredNoiseDataSpecular;
#if _USE_OPTIMIZATION_DEFINES            
#ifdef _SPECULAR_STYLING_RANDOMIZER_PERLIN
                requiredNoiseDataSpecular.perlinNoise = 1;
#else
                requiredNoiseDataSpecular.perlinNoise = 0;
#endif
#ifdef _SPECULAR_STYLING_RANDOMIZER_PERLIN_FLOORED
                requiredNoiseDataSpecular.perlinNoiseFloored = 1;
#else
                requiredNoiseDataSpecular.perlinNoiseFloored = 0;
#endif         
#ifdef _SPECULAR_STYLING_RANDOMIZER_WHITE
                requiredNoiseDataSpecular.whiteNoise = 1;
#else
                requiredNoiseDataSpecular.whiteNoise = 0;
#endif
#ifdef _SPECULAR_STYLING_RANDOMIZER_WHITE_FLOORED
                requiredNoiseDataSpecular.whiteNoiseFloored = 1;
#else
                requiredNoiseDataSpecular.whiteNoiseFloored = 0;
#endif      
#else            
                requiredNoiseDataSpecular.perlinNoise = 1;
                requiredNoiseDataSpecular.perlinNoiseFloored = 1;
                requiredNoiseDataSpecular.whiteNoise = 1;
                requiredNoiseDataSpecular.whiteNoiseFloored = 1;
#endif
#if _URP
                float2 llllllllllllllllllllllllllllll21 = ConvertToDrawSpace(inputData, lllllllll1, uvSpaceDataSpecular, llllllllllllllllllllllllllll0, uvSets);
#else
                float2 llllllllllllllllllllllllllllll21 = ConvertToDrawSpace(d.worldSpacePosition, d.worldSpaceNormal, lllllllll1, uvSpaceDataSpecular, llllllllllllllllllllllllllll0, uvSets);
#endif
            llllllllllllllllllllllllllllll21 = PixelateDrawSpaceUV(llllllllllllllllllllllllllllll21, uvSpaceDataSpecular, llllllllllllllllllllllllllll12, lllllllllllllllllllllllllllll12);
            half ll22 = llllllllllllllllllllllllllll12 == 1 ? 0 : lllllllllll8;
                float2 lllllll5 = RotateUV(llllllllllllllllllllllllllllll21, stylingDataSpecular.rotation);
                llllllllllllllllllllllllllllll21 = lllllll5;
                NoiseSampleData noiseSampleData = SampleNoiseData(llllllllllllllllllllllllllllll21, stylingDataSpecular, stylingRandomDataSpecular, requiredNoiseDataSpecular, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
#if _USE_OPTIMIZATION_DEFINES 
#ifdef _SPECULAR_STYLE
            stylingDataSpecular.style = _SPECULAR_STYLE;
#endif
#endif
                float llllllllllllllllllllll18 = 0;
                if (stylingDataSpecular.style == 0) 
                {
                    llllllllllllllllllllll18 = Hatching(llll14, llllllllllllllllllllllllllllll21, stylingDataSpecular, stylingRandomDataSpecular, noiseSampleData, ll22);
                    llllllllllllllllllllll18 = 1 - llllllllllllllllllllll18;
                }
                else if (stylingDataSpecular.style == 1) 
                {
                    float lllllllllllllllllllllllllllll9 = Halftones(llll14, llllllllllllllllllllllllllllll21, stylingDataSpecular, stylingRandomDataSpecular, noiseSampleData, ll22);
                    llllllllllllllllllllll18 = lllllllllllllllllllllllllllll9;
                }
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SPECULAR_STYLING_BLENDING
                     positionAndBlendingDataSpecular.blending = _SPECULAR_STYLING_BLENDING;
#endif
#endif
                half4 lll18;
                if (llllllllllllllllllllllllll12 == 1)
                {
                    lll18 = half4(lllll14, 1);
                }
                else
                {
                    lll18 = stylingDataSpecular.color;
                }
                DoBlending(l10, 1 - llllllllllllllllllllll18, positionAndBlendingDataSpecular.blending, lll18);
            }
#endif
        }
#if _ENABLE_RIM_STYLING || !_USE_OPTIMIZATION_DEFINES   
#if !_USE_OPTIMIZATION_DEFINES
        if (llllllllllllllllllllllllllllll12 && llll13 != 1)
#else
        if (llll13 != 1)
#endif
        {
#if _USE_OPTIMIZATION_DEFINES
#ifdef _RIM_STYLING_BLENDING
                    positionAndBlendingDataRim.blending = _RIM_STYLING_BLENDING;
#endif
#ifdef _RIM_STYLING_DRAWSPACE
                uvSpaceDataRim.drawSpace = _RIM_STYLING_DRAWSPACE;
#endif
#ifdef _RIM_STYLING_COORDINATESYSTEM
                uvSpaceDataRim.coordinateSystem = _RIM_STYLING_COORDINATESYSTEM;
#endif        
#ifdef _RIM_STYLE
                stylingDataRim.style = _RIM_STYLE;
#endif
#if _RIM_STYLING_RANDOMIZER
                stylingRandomDataRim.enableRandomizer = 1;
#else
                stylingRandomDataRim.enableRandomizer = 0;
#endif
#endif
            RequiredNoiseData requiredNoiseDataRim;
#if _USE_OPTIMIZATION_DEFINES
#ifdef _RIM_STYLING_RANDOMIZER_PERLIN
                requiredNoiseDataRim.perlinNoise = 1;
#else
                requiredNoiseDataRim.perlinNoise = 0;
#endif
#ifdef _RIM_STYLING_RANDOMIZER_PERLIN_FLOORED
                requiredNoiseDataRim.perlinNoiseFloored = 1;
#else
                requiredNoiseDataRim.perlinNoiseFloored = 0;
#endif         
#ifdef _RIM_STYLING_RANDOMIZER_WHITE
                requiredNoiseDataRim.whiteNoise = 1;
#else
                requiredNoiseDataRim.whiteNoise = 0;
#endif
#ifdef _RIM_STYLING_RANDOMIZER_WHITE_FLOORED
                requiredNoiseDataRim.whiteNoiseFloored = 1;
#else
                requiredNoiseDataRim.whiteNoiseFloored = 0;
#endif      
#else            
            requiredNoiseDataRim.perlinNoise = 1;
            requiredNoiseDataRim.perlinNoiseFloored = 1;
            requiredNoiseDataRim.whiteNoise = 1;
            requiredNoiseDataRim.whiteNoiseFloored = 1;
#endif
#if _URP
            float2 lllllll22 = ConvertToDrawSpace(inputData, lllllllll1, uvSpaceDataRim, llllllllllllllllllllllllllll0, uvSets);
#else
            float2 lllllll22 = ConvertToDrawSpace(d.worldSpacePosition, d.worldSpaceNormal, lllllllll1, uvSpaceDataRim, llllllllllllllllllllllllllll0, uvSets);
#endif
            lllllll22 = PixelateDrawSpaceUV(lllllll22, uvSpaceDataRim, llllll13, lllllll13);
            half llllllllll22 = llllll13 == 1 ? 0 : lllllllllll8;
            float2 lllllll5 = RotateUV(lllllll22, stylingDataRim.rotation);
            NoiseSampleData noiseSampleData = SampleNoiseData(lllllll5, stylingDataRim, stylingRandomDataRim, requiredNoiseDataRim, lllllllllllllllllllllllllllll7, llllllllllllllllllllllllllllll7);
            float llllllllllllllllllllll18 = 0;
            if (stylingDataRim.style == 0) 
            {
                llllllllllllllllllllll18 = Hatching(lllll18, lllllll5, stylingDataRim, stylingRandomDataRim, noiseSampleData, llllllllll22);
                llllllllllllllllllllll18 = 1 - llllllllllllllllllllll18;
            }
            else if (stylingDataRim.style == 1) 
            {
                float lllllllllllllllllllllllllllll9 = Halftones(lllll18, lllllll5, stylingDataRim, stylingRandomDataRim, noiseSampleData, llllllllll22);
                llllllllllllllllllllll18 = lllllllllllllllllllllllllllll9;
            }
            DoBlending(l10, 1 - llllllllllllllllllllll18, positionAndBlendingDataRim.blending, stylingDataRim.color);
        }
#endif
    }
#endif


}

    
    
    
    
    
    
    
    
    
    
    
    
    
    

        


void AddTheToonShader(inout float4 albedo,

#if _URP
    InputData inputData, 
    SurfaceData surface,
#else
#if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
                 SurfaceOutputStandardSpecular o,
#elif _BDRFLAMBERT || _BDRF3 || _SIMPLELIT

                 SurfaceOutput o,
#else
                 SurfaceOutputStandard o,
#endif

    UnityGI gi,
#if !_PASSFORWARDADD
    UnityGIInput giInput,
#endif
#endif

 ShaderData d
#if _URP
#if UNITY_VERSION >= 202120
, float3 normalTS
#endif
#endif

)
{
    
    float2 uv = d.texcoord0.xy;
    
    UVSets uvSets;
    uvSets.uv0 = d.texcoord0.xy;
    uvSets.uv1 = d.texcoord1.xy;
    uvSets.uv2 = d.texcoord2.xy;
    uvSets.uv3 = d.texcoord3.xy;
    
    
    
        
    

    
    float3 pureNormal = d.worldSpaceNormal;

    float4 screenUV = d.extraV2F0;

    

    
    UVSpaceData uvSpaceDataShading;
    uvSpaceDataShading.drawSpace = _DrawSpace;
    uvSpaceDataShading.uvSet = _UVSet;
    uvSpaceDataShading.coordinateSystem = _CoordinateSystem;
    uvSpaceDataShading.polarCenterMode = _PolarCenterMode;
    uvSpaceDataShading.polarCenter = _PolarCenter;
    uvSpaceDataShading.sSCameraDistanceScaled = _SSCameraDistanceScaled;
    uvSpaceDataShading.anchorSSToObjectsOrigin = _AnchorSSToObjectsOrigin;
    
     
    
    UVSpaceData uvSpaceDataCastShadows;
    uvSpaceDataCastShadows.drawSpace = _CastShadowsDrawSpace;
    uvSpaceDataCastShadows.uvSet = _CastShadowsUVSet;
    uvSpaceDataCastShadows.coordinateSystem = _CastShadowsCoordinateSystem;
    uvSpaceDataCastShadows.polarCenterMode = _CastShadowsPolarCenterMode;
    uvSpaceDataCastShadows.polarCenter = _CastShadowsPolarCenter;
    uvSpaceDataCastShadows.sSCameraDistanceScaled = _CastShadowsSSCameraDistanceScaled;
    uvSpaceDataCastShadows.anchorSSToObjectsOrigin = _CastShadowsAnchorSSToObjectsOrigin;
    
    UVSpaceData uvSpaceDataSpecular;
    uvSpaceDataSpecular.drawSpace = _SpecularDrawSpace;
    uvSpaceDataSpecular.uvSet = _SpecularUVSet;
    uvSpaceDataSpecular.coordinateSystem = _SpecularCoordinateSystem;
    uvSpaceDataSpecular.polarCenterMode = _SpecularPolarCenterMode;
    uvSpaceDataSpecular.polarCenter = _SpecularPolarCenter;
    uvSpaceDataSpecular.sSCameraDistanceScaled = _SpecularSSCameraDistanceScaled;
    uvSpaceDataSpecular.anchorSSToObjectsOrigin = _SpecularAnchorSSToObjectsOrigin;
    
    UVSpaceData uvSpaceDataRim;
    uvSpaceDataRim.drawSpace = _RimDrawSpace;
    uvSpaceDataRim.uvSet = _RimUVSet;
    uvSpaceDataRim.coordinateSystem = _RimCoordinateSystem;
    uvSpaceDataRim.polarCenterMode = _RimPolarCenterMode;
    uvSpaceDataRim.polarCenter = _RimPolarCenter;
    uvSpaceDataRim.sSCameraDistanceScaled = _RimSSCameraDistanceScaled;
    uvSpaceDataRim.anchorSSToObjectsOrigin = _RimAnchorSSToObjectsOrigin;

    GeneralStylingData generalStylingData;
    generalStylingData.enableDistanceFade = _EnableStylingDistanceFade;
    generalStylingData.distanceFadeStartDistance = _StylingDFStartingDistance;
    generalStylingData.distanceFadeFalloff = _StylingDFFalloff;
    generalStylingData.adjustDistanceFadeValue = _StylingAdjustDistanceFadeValue;
    generalStylingData.distanceFadeValue = _StylingDistanceFadeValue;
    StylingData stylingDataShading;
    stylingDataShading.style = _ShadingStyle;
    stylingDataShading.type = 0;
    stylingDataShading.color = _StylingColor;
    stylingDataShading.rotation = _StylingShadingInitialDirection;
    stylingDataShading.rotationBetweenCells = _StylingShadingRotationBetweenCells;
    stylingDataShading.density = _StylingShadingDensity;
    stylingDataShading.offset = _StylingShadingHalftonesOffset;
    stylingDataShading.size = _StylingShadingThickness;
    stylingDataShading.sizeMin = _StylingShadingThicknessMin;
    stylingDataShading.sizeMinFromControlMap = _StylingShadingUseControlMapThickness;
    stylingDataShading.sizeControl = _StylingShadingThicknessControl;
    stylingDataShading.sizeFalloff = _StylingShadingThicknessFalloff;
    stylingDataShading.roundness = _StylingShadingHalftonesRoundness;
    stylingDataShading.roundnessFalloff = _StylingShadingHalftonesRoundnessFalloff;
    stylingDataShading.hardness = _StylingShadingHardness;
    stylingDataShading.opacity = _StylingShadingOpacity;
    stylingDataShading.opacityFalloff = _StylingShadingOpacityFalloff;
    
    
    
    stylingDataShading.dashEnabled = _StylingShadingEnableDashes;
    stylingDataShading.dashType = _StylingShadingDashesType;
    stylingDataShading.dashLength = _StylingShadingDashesSize;
    stylingDataShading.dashDensity = _StylingShadingDashesUseHatchingDensity == 1 ? _StylingShadingDensity : _StylingShadingDashesDensity;
    stylingDataShading.dashTransitionPosition = _StylingShadingDashesTransitionPosition;
    stylingDataShading.dashTransitionSoftness = _StylingShadingDashesTransitionSoftness;
    stylingDataShading.dashRoundness = _StylingShadingDashesRoundness;
    stylingDataShading.dashOffset = _StylingShadingDashesOffset;
    

    StylingData stylingDataSpecular;
    stylingDataSpecular.style = _SpecularStyle;
    stylingDataSpecular.type = 1;
    stylingDataSpecular.color = _StylingSpecularColor;
    stylingDataSpecular.rotation = _StylingSpecularRotation;
    stylingDataSpecular.density = _StylingSpecularDensity;
    stylingDataSpecular.offset = _StylingSpecularHalftonesOffset;
    stylingDataSpecular.size = _StylingSpecularThickness;
    stylingDataSpecular.sizeMin = 0;
    stylingDataSpecular.sizeMinFromControlMap = 0;
    stylingDataSpecular.sizeControl = _StylingSpecularThicknessControl;
    stylingDataSpecular.sizeFalloff = _StylingSpecularThicknessFalloff;
    stylingDataSpecular.roundness = _StylingSpecularHalftonesRoundness;
    stylingDataSpecular.roundnessFalloff = _StylingSpecularHalftonesRoundnessFalloff;
    stylingDataSpecular.hardness = _StylingSpecularHardness;
    stylingDataSpecular.opacity = _StylingSpecularOpacity;
    stylingDataSpecular.opacityFalloff = _StylingSpecularOpacityFalloff;

    
    stylingDataSpecular.dashEnabled = _StylingSpecularEnableDashes;
    stylingDataSpecular.dashType = _StylingSpecularDashesType;
    stylingDataSpecular.dashLength = _StylingSpecularDashesSize;
    stylingDataSpecular.dashDensity = _StylingSpecularDashesUseHatchingDensity == 1 ? _StylingSpecularDensity : _StylingSpecularDashesDensity;
    stylingDataSpecular.dashTransitionPosition = _StylingSpecularDashesTransitionPosition;
    stylingDataSpecular.dashTransitionSoftness = _StylingSpecularDashesTransitionSoftness;
    stylingDataSpecular.dashRoundness = _StylingSpecularDashesRoundness;
    stylingDataSpecular.dashOffset = _StylingSpecularDashesOffset;
    

    
    StylingData stylingDataCastShadows;
    
    stylingDataCastShadows.style = _CastShadowsStyle;
    stylingDataCastShadows.type = 1;
    stylingDataCastShadows.color = _StylingCastShadowsColor;
    stylingDataCastShadows.rotation = _StylingCastShadowsInitialDirection;
    stylingDataCastShadows.rotationBetweenCells = _StylingCastShadowsRotationBetweenCells;
    stylingDataCastShadows.density = _StylingCastShadowsDensity;
    stylingDataCastShadows.offset = _StylingCastShadowsHalftonesOffset;
    stylingDataCastShadows.size = _StylingCastShadowsThickness;
    stylingDataCastShadows.sizeMin = 0;
    stylingDataCastShadows.sizeMinFromControlMap = 0;
    stylingDataCastShadows.sizeControl = _StylingCastShadowsThicknessControl;
    stylingDataCastShadows.sizeFalloff = _StylingCastShadowsThicknessFalloff;
    stylingDataCastShadows.roundness = _StylingCastShadowsHalftonesRoundness;
    stylingDataCastShadows.roundnessFalloff = _StylingCastShadowsHalftonesRoundnessFalloff;
    stylingDataCastShadows.hardness = _StylingCastShadowsHardness;
    stylingDataCastShadows.opacity = _StylingCastShadowsOpacity;
    stylingDataCastShadows.opacityFalloff = _StylingCastShadowsOpacityFalloff;

    
    stylingDataCastShadows.dashEnabled = _StylingCastShadowsEnableDashes;
    stylingDataCastShadows.dashType = _StylingCastShadowsDashesType;
    stylingDataCastShadows.dashLength = _StylingCastShadowsDashesSize;
    stylingDataCastShadows.dashDensity = _StylingCastShadowsDashesUseHatchingDensity == 1 ? _StylingCastShadowsDensity : _StylingCastShadowsDashesDensity;
    stylingDataCastShadows.dashTransitionPosition = _StylingCastShadowsDashesTransitionPosition;
    stylingDataCastShadows.dashTransitionSoftness = _StylingCastShadowsDashesTransitionSoftness;
    stylingDataCastShadows.dashRoundness = _StylingCastShadowsDashesRoundness;
    stylingDataCastShadows.dashOffset = _StylingCastShadowsDashesOffset;
    

    StylingData stylingDataRim;
    stylingDataRim.style = _RimStyle;
    stylingDataRim.type = 1;
    stylingDataRim.color = _StylingRimColor;
    stylingDataRim.rotation = _StylingRimRotation;
    stylingDataRim.density = _StylingRimDensity;
    stylingDataRim.offset = _StylingRimHalftonesOffset;
    stylingDataRim.size = _StylingRimThickness;
    stylingDataRim.sizeMin = 0;
    stylingDataRim.sizeMinFromControlMap = 0;
    stylingDataRim.sizeControl = _StylingRimThicknessControl;
    stylingDataRim.sizeFalloff = _StylingRimThicknessFalloff;
    stylingDataRim.roundness = _StylingRimHalftonesRoundness;
    stylingDataRim.roundnessFalloff = _StylingRimHalftonesRoundnessFalloff;
    stylingDataRim.hardness = _StylingRimHardness;
    stylingDataRim.opacity = _StylingRimOpacity;
    stylingDataRim.opacityFalloff = _StylingRimOpacityFalloff;

    
    stylingDataRim.dashEnabled = _StylingRimEnableDashes;
    stylingDataRim.dashType = _StylingRimDashesType;
    stylingDataRim.dashLength = _StylingRimDashesSize;
    stylingDataRim.dashDensity = _StylingRimDashesUseHatchingDensity == 1 ? _StylingRimDensity : _StylingRimDashesDensity;
    stylingDataRim.dashTransitionPosition = _StylingRimDashesTransitionPosition;
    stylingDataRim.dashTransitionSoftness = _StylingRimDashesTransitionSoftness;
    stylingDataRim.dashRoundness = _StylingRimDashesRoundness;
    stylingDataRim.dashOffset = _StylingRimDashesOffset;
    

    
 
    
    PositionAndBlendingData positionAndBlendingDataShading;
            
    positionAndBlendingDataShading.blending = _StylingShadingBlending;
    positionAndBlendingDataShading.isInverted = _StylingShadingIsInverted;

    PositionAndBlendingData positionAndBlendingDataSpecular;
            
    positionAndBlendingDataSpecular.blending = _StylingSpecularBlending;
    positionAndBlendingDataSpecular.isInverted = _StylingSpecularIsInverted;
    
    PositionAndBlendingData positionAndBlendingDataCastShadows;
    positionAndBlendingDataCastShadows.blending = _StylingCastShadowsBlending;
    positionAndBlendingDataCastShadows.isInverted = _StylingCastShadowsIsInverted;
    
    PositionAndBlendingData positionAndBlendingDataRim;
            
    positionAndBlendingDataRim.blending = _StylingRimBlending;
    positionAndBlendingDataRim.isInverted = _StylingRimIsInverted;



    StylingRandomData stylingRandomDataShading;
    stylingRandomDataShading.enableRandomizer = _EnableShadingRandomizer;
    stylingRandomDataShading.perlinNoiseSize = _ShadingNoise1Size;
    stylingRandomDataShading.perlinNoiseSeed = _ShadingNoise1Seed;
    stylingRandomDataShading.whiteNoiseSeed = _ShadingNoise2Seed;
    stylingRandomDataShading.noiseIntensity = _NoiseIntensity;
    stylingRandomDataShading.spacingRandomMode = _SpacingRandomMode;
    stylingRandomDataShading.spacingRandomIntensity = _SpacingRandomIntensity;
    stylingRandomDataShading.opacityRandomMode = _OpacityRandomMode;
    stylingRandomDataShading.opacityRandomIntensity = _OpacityRandomIntensity;
    stylingRandomDataShading.lengthRandomMode = _LengthRandomMode;
    stylingRandomDataShading.lengthRandomIntensity = _LengthRandomIntensity;
    stylingRandomDataShading.hardnessRandomMode = _HardnessRandomMode;
    stylingRandomDataShading.hardnessRandomIntensity = _HardnessRandomIntensity;
    stylingRandomDataShading.thicknessRandomMode = _ThicknessRandomMode;
    stylingRandomDataShading.thicknesshRandomIntensity = _ThicknesshRandomIntensity;
    
    
    
    StylingRandomData stylingRandomDataSpecular;
    stylingRandomDataSpecular.enableRandomizer = _EnableSpecularRandomizer;
    stylingRandomDataSpecular.perlinNoiseSize = _SpecularNoise1Size;
    stylingRandomDataSpecular.perlinNoiseSeed = _SpecularNoise1Seed;
    stylingRandomDataSpecular.whiteNoiseSeed = _SpecularNoise2Seed;
    stylingRandomDataSpecular.noiseIntensity = _SpecularNoiseIntensity;
    stylingRandomDataSpecular.spacingRandomMode = _SpecularSpacingRandomMode;
    stylingRandomDataSpecular.spacingRandomIntensity = _SpecularSpacingRandomIntensity;
    stylingRandomDataSpecular.opacityRandomMode = _SpecularOpacityRandomMode;
    stylingRandomDataSpecular.opacityRandomIntensity = _SpecularOpacityRandomIntensity;
    stylingRandomDataSpecular.lengthRandomMode = _SpecularLengthRandomMode;
    stylingRandomDataSpecular.lengthRandomIntensity = _SpecularLengthRandomIntensity;
    stylingRandomDataSpecular.hardnessRandomMode = _SpecularHardnessRandomMode;
    stylingRandomDataSpecular.hardnessRandomIntensity = _SpecularHardnessRandomIntensity;
    stylingRandomDataSpecular.thicknessRandomMode = _SpecularThicknessRandomMode;
    stylingRandomDataSpecular.thicknesshRandomIntensity = _SpecularThicknesshRandomIntensity;
    
    StylingRandomData stylingRandomDataCastShadows;
    stylingRandomDataCastShadows.enableRandomizer = _EnableCastShadowsRandomizer;
    stylingRandomDataCastShadows.perlinNoiseSize = _CastShadowsNoise1Size;
    stylingRandomDataCastShadows.perlinNoiseSeed = _CastShadowsNoise1Seed;
    stylingRandomDataCastShadows.whiteNoiseSeed = _CastShadowsNoise2Seed;
    stylingRandomDataCastShadows.noiseIntensity = _CastShadowsNoiseIntensity;
    stylingRandomDataCastShadows.spacingRandomMode = _CastShadowsSpacingRandomMode;
    stylingRandomDataCastShadows.spacingRandomIntensity = _CastShadowsSpacingRandomIntensity;
    stylingRandomDataCastShadows.opacityRandomMode = _CastShadowsOpacityRandomMode;
    stylingRandomDataCastShadows.opacityRandomIntensity = _CastShadowsOpacityRandomIntensity;
    stylingRandomDataCastShadows.lengthRandomMode = _CastShadowsLengthRandomMode;
    stylingRandomDataCastShadows.lengthRandomIntensity = _CastShadowsLengthRandomIntensity;
    stylingRandomDataCastShadows.hardnessRandomMode = _CastShadowsHardnessRandomMode;
    stylingRandomDataCastShadows.hardnessRandomIntensity = _CastShadowsHardnessRandomIntensity;
    stylingRandomDataCastShadows.thicknessRandomMode = _CastShadowsThicknessRandomMode;
    stylingRandomDataCastShadows.thicknesshRandomIntensity = _CastShadowsThicknesshRandomIntensity;

    StylingRandomData stylingRandomDataRim;
    stylingRandomDataRim.enableRandomizer = _EnableRimRandomizer;
    stylingRandomDataRim.perlinNoiseSize = _RimNoise1Size;
    stylingRandomDataRim.perlinNoiseSeed = _RimNoise1Seed;
    stylingRandomDataRim.whiteNoiseSeed = _RimNoise2Seed;
    stylingRandomDataRim.noiseIntensity = _RimNoiseIntensity;
    stylingRandomDataRim.spacingRandomMode = _RimSpacingRandomMode;
    stylingRandomDataRim.spacingRandomIntensity = _RimSpacingRandomIntensity;
    stylingRandomDataRim.opacityRandomMode = _RimOpacityRandomMode;
    stylingRandomDataRim.opacityRandomIntensity = _RimOpacityRandomIntensity;
    stylingRandomDataRim.lengthRandomMode = _RimLengthRandomMode;
    stylingRandomDataRim.lengthRandomIntensity = _RimLengthRandomIntensity;
    stylingRandomDataRim.hardnessRandomMode = _RimHardnessRandomMode;
    stylingRandomDataRim.hardnessRandomIntensity = _RimHardnessRandomIntensity;
    stylingRandomDataRim.thicknessRandomMode = _RimThicknessRandomMode;
    stylingRandomDataRim.thicknesshRandomIntensity = _RimThicknesshRandomIntensity;


    DoToonShading(
#if _URP
            inputData,
            surface,
#else
            o,
            gi,
#if !_PASSFORWARDADD
            giInput,
#endif
#endif
            d,
#if _URP
#if UNITY_VERSION >= 202120
            normalTS,
#endif
#endif    
            albedo, _NumberOfCells, _CellTransitionSmoothness, _SumLightsBeforePosterization, _ShadingUseLightColors,
    
            uv, screenUV, _HatchingMap,
            
            _ShadingMode, _LightFunction,

            _EnableToonShading, _ShadingFunction,

            _GradientTex, _GradientTex_TexelSize, _GradientMode, _GradientBlending, _GradientBlendFactor,

            _EnableShadows, _CoreShadowColor,
    
            _TerminatorPosition,
    
            _TerminatorWidth, _TerminatorSmoothness, _FormShadowColor,
            _EnableCastShadows, _CastShadowsStrength, _CastShadowsSmoothness, _CastShadowColorMode, _CastShadowColor,
            _ShadingAffectedByNormalMap,
    
            _EnableSpecular, _SpecularBlending, _SpecularColor, _SpecularSize, _SpecularSmoothness, _SpecularOpacity, _SpecularAffectedByNormalMap, _SpecularUseLightColors,
            
            _EnableRim, _RimBlending, _RimColor, _RimSize, _RimSmoothness, _RimOpacity, _RimAffectedArea, _RimAffectedByNormalMap,
            
    
            _EnableStyling,
    
            uvSets,
    
            generalStylingData, _HatchingAffectedByNormalMap, _EnableAntiAliasing,
    
            _EnableShadingStyling,
            _StylingShadingSyncWithOtherStyling,
            _SyncWithLightPartitioning, _NumberOfCellsHatching,
            _StylingTerminatorPosition,
            _StylingOvermodelingFactor,
            _StylingShadingEnableMappingPixelation, _StylingShadingMappingPixelSize,
            positionAndBlendingDataShading, uvSpaceDataShading, stylingDataShading, stylingRandomDataShading,
    
            _EnableCastShadowsStyling,
            _StylingCastShadowsSyncWithOtherStyling,
            _CastShadowsNumberOfCellsHatching, _StylingCastShadowsSmoothness,
            _StylingCastShadowsEnableMappingPixelation, _StylingCastShadowsMappingPixelSize,
            positionAndBlendingDataCastShadows, uvSpaceDataCastShadows, stylingDataCastShadows, stylingRandomDataCastShadows,
    
            _EnableSpecularStyling,
            _SyncWithSpecular, _StylingSpecularSize, _StylingSpecularSmoothness, _StylingSpecularShadingInteraction, _StylingSpecularUseLightColors,
            _StylingSpecularSyncWithOtherStyling,
            _StylingSpecularEnableMappingPixelation, _StylingSpecularMappingPixelSize,
            positionAndBlendingDataSpecular, uvSpaceDataSpecular, stylingDataSpecular, stylingRandomDataSpecular,
    
            _EnableRimStyling,
            _SyncWithRim, _StylingRimSize, _StylingRimSmoothness, _StylingRimAffectedArea,
            _StylingRimShadingInteraction,
            _StylingRimSyncWithOtherStyling,
            _StylingRimEnableMappingPixelation, _StylingRimMappingPixelSize,
            positionAndBlendingDataRim, uvSpaceDataRim, stylingDataRim, stylingRandomDataRim,


            _NoiseMap1, _NoiseMap2, _NoiseTex2_TexelSize,
            
            pureNormal);
    
}










#endif

