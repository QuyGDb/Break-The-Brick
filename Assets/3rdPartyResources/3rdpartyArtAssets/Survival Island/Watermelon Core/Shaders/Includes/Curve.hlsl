#ifndef W_CURVE
#define W_CURVE

    uniform half _CurveForwardOffset;
    uniform half _CurveForwardPower;

    uniform half _CurveSideOffset;
    uniform half _CurveSidePower;

    uniform half3 _PlayerPosition;

    float4 GetCurveOffset(float3 positionWS)
    {
#ifdef CURVE_ON
        half forwardPos = positionWS.z - _PlayerPosition.z - _CurveForwardOffset;
        half forwardTemp = forwardPos / 32;
        forwardTemp = forwardTemp * forwardTemp * _CurveForwardPower * (forwardPos > 0);

        half rightPos = positionWS.x - _PlayerPosition.x - _CurveSideOffset;
        half rightTemp = rightPos / 32;
        rightTemp = rightTemp * rightTemp * _CurveSidePower * (rightPos > 0);

        half leftPos = positionWS.x - _PlayerPosition.x + _CurveSideOffset;
        half leftTemp = leftPos / 32;
        leftTemp = leftTemp * leftTemp * _CurveSidePower * (leftPos < 0);

        half4 offset = mul(unity_WorldToObject, half4(0, forwardTemp + rightTemp + leftTemp, 0, 0));

        return offset;
#endif
        return 0;
    }

    ////// SHADERGRAPH ///////

    void GetCurveOffset_half(in half3 positionWS, out half4 offsetOS)
    {
        offsetOS = GetCurveOffset(positionWS);
    }

    void GetCurveOffset_float(in float3 positionWS, out float4 offsetOS)
    {
        offsetOS = GetCurveOffset(positionWS);
    }

#endif