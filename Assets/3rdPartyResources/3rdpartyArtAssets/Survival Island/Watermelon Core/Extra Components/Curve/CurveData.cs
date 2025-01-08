using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurveData
    {
        [SerializeField] float curveForwardOffset = 8;
        [SerializeField] float curveForwardPower = 18;

        public float ForwardPower => curveForwardPower;
        public float ForwardOffset => curveForwardOffset;

        [Space]
        [SerializeField] float curveSideOffset = 0;
        [SerializeField] float curveSidePower = 0;

        public float SideOffset => curveSideOffset;
        public float SidePower => curveSidePower;

        [Space]
        [SerializeField] float fovMultiplier = 1.5f;
        [SerializeField] float aspectMultiplier = 1.3f;

        public float FovMultiplier => fovMultiplier;
        public float AspectMultiplier => aspectMultiplier;

    }
}
