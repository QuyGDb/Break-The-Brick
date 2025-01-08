using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Weather Preset", menuName = "Content/Weather/Weather Preset")]
    public class EnvironmentWeatherPreset : ScriptableObject
    {
        [Header("Light")]
        [SerializeField] Blending lightBlending;
        public Blending LightBlending => lightBlending;
        [SerializeField, HideIf("IsLightBlendingNone")] Color lightColor = Color.white;
        public Color LightColor => lightColor;

        [Header("Sky")]
        [SerializeField] Blending skyBlending;
        public Blending SkyBlending => skyBlending;
        [SerializeField, HideIf("IsSkyBlendingNone")] Gradient skyGradient;
        public Gradient SkyGradient => skyGradient;

        [Header("Shadows")]
        [SerializeField] Blending shadowsBlending;
        public Blending ShadowsBlending => shadowsBlending;
        [SerializeField, HideIf("IsShadowsBlendingNone"), Range(0, 2)] float shadowsIntensity = 1;
        public float ShadowsIntensity => shadowsIntensity;

        [Header("Rim")]
        [SerializeField] Blending rimBlending;
        public Blending RimBlending => rimBlending;
        [SerializeField, Range(0, 2), HideIf("IsRimBlendingNone")] float rimIntensity = 1;
        public float RimIntensity => rimIntensity;

        [Header("Wind")]
        [SerializeField] Blending windBlending;
        public Blending WindBlending => windBlending;
        [SerializeField, Range(0, 10), HideIf("IsWindBlendingNone")] float windMultiplier = 1;
        public float WindMultiplier => windMultiplier;

        [Header("Clouds")]
        [SerializeField] Blending cloudsBlending;
        public Blending CloudsBlending => cloudsBlending;
        [SerializeField, Range(0, 1), HideIf("IsCloudsBlendingNone")] float cloudsInfluence1 = 1;
        public float CloudsInfluence1 => cloudsInfluence1;
        [SerializeField, Range(0, 1), HideIf("IsCloudsBlendingNone")] float cloudsInfluence2 = 1;
        public float CloudsInfluence2 => cloudsInfluence2;

        [Header("Particle")]
        [SerializeField] GameObject weatherParticle;
        public GameObject WeatherParticle => weatherParticle;

        [Header("Fog")]
        [SerializeField] bool fogEnabled;
        public bool FogEnabled => fogEnabled;

        [SerializeField] Color fogColor;
        public Color FogColor => fogColor;

        [SerializeField, Range(0, 1)] float fogDensity;
        public float FogDensity => fogDensity;

        [SerializeField] FogMode fogMode = FogMode.ExponentialSquared;
        public FogMode FogMode => fogMode;

        [SerializeField, ShowIf("IsFogModeLinear")] DuoFloat forRange;
        public DuoFloat FogRange => forRange;


        public Color GetLightColor(Color color)
        {
            switch(lightBlending)
            {
                case Blending.Additive: return color + lightColor;
                case Blending.Subtract: return color - lightColor;
                case Blending.Override: return lightColor;
                case Blending.Multiply: return color * lightColor;

                default: return color;
            }
        }

        public float GetShadowsMultiplier(float shadows)
        {
            switch (shadowsBlending)
            {
                case Blending.Additive: return shadows + shadowsIntensity;
                case Blending.Subtract: return shadows - shadowsIntensity;
                case Blending.Override: return shadowsIntensity;
                case Blending.Multiply: return shadows * shadowsIntensity;

                default: return shadows;
            }
        }

        public float GetRimMultiplier(float rim)
        {
            switch (rimBlending)
            {
                case Blending.Additive: return rim + rimIntensity;
                case Blending.Subtract: return rim - rimIntensity;
                case Blending.Override: return rimIntensity;
                case Blending.Multiply: return rim * rimIntensity;

                default: return rim;
            }
        }

        public Color SampleSkyGradient(Color color, float t)
        {
            var skyColor = skyGradient.Evaluate(t);
            switch (skyBlending)
            {
                case Blending.Additive: return (color + skyColor).SetAlpha(color.a);
                case Blending.Subtract: return (color - skyColor).SetAlpha(color.a);
                case Blending.Override: return skyColor;
                case Blending.Multiply: return color * skyColor;

                default: return color;
            }
        }

        public float GetWindMultiplier(float wind)
        {
            switch (windBlending)
            {
                case Blending.Additive: return wind + windMultiplier;
                case Blending.Subtract: return wind - windMultiplier;
                case Blending.Override: return windMultiplier;
                case Blending.Multiply: return wind * windMultiplier;

                default: return wind;
            }
        }

        public DuoFloat GetCloudsInfluence(DuoFloat clouds)
        {
            var weatherClouds = new DuoFloat(cloudsInfluence1, cloudsInfluence2);

            switch (cloudsBlending)
            {
                case Blending.Additive: return clouds + weatherClouds;
                case Blending.Subtract: return clouds - weatherClouds;
                case Blending.Override: return weatherClouds;
                case Blending.Multiply: return clouds * weatherClouds;

                default: return clouds;
            }
        }

        private bool IsLightBlendingNone() => lightBlending == Blending.None;
        private bool IsShadowsBlendingNone() => shadowsBlending == Blending.None;
        private bool IsRimBlendingNone() => rimBlending == Blending.None;
        private bool IsSkyBlendingNone() => skyBlending == Blending.None;
        private bool IsWindBlendingNone() => windBlending == Blending.None;
        private bool IsCloudsBlendingNone() => cloudsBlending == Blending.None;
        private bool IsFogModeLinear() => fogMode == FogMode.Linear;

        [Button]
        public void ApplyWeatherPreset()
        {
            if (!Application.isPlaying) return;

            EnvironmentController.ApplyWeatherDev(this);
        }
    }

    public enum Blending
    {
        None,
        Override,
        Multiply,
        Additive,
        Subtract,
    }
}
