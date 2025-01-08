using UnityEngine;

namespace Watermelon
{
    public class EnvironmentWeatherModule
    {
        private EnvironmentWeatherPreset CurrentWeatherPreset { get; set; }
        private EnvironmentWeatherPreset OldWeatherPreset { get; set; }
        private float t;

        public bool IsTransitioning { get; private set; }

        private ParticleSystem WeatherParticle { get; set; }
        private ParticleSystem OldWeatherParticle { get; set; }

        public void SetWeatherPreset(EnvironmentWeatherPreset preset, float transitionDuration)
        {
            if (CurrentWeatherPreset != null)
            {
                OldWeatherPreset = CurrentWeatherPreset;
            }
            CurrentWeatherPreset = preset;

            if (OldWeatherPreset != null && WeatherParticle != null)
            {
                OldWeatherParticle = WeatherParticle;
                OldWeatherParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Tween.DelayedCall(OldWeatherParticle.main.duration, () =>
                {
                    Object.Destroy(OldWeatherParticle.gameObject);
                });
            }

            if (CurrentWeatherPreset.WeatherParticle)
            {
                WeatherParticle = Object.Instantiate(CurrentWeatherPreset.WeatherParticle).GetComponent<ParticleSystem>();

                WeatherParticle.transform.SetParent(Camera.main.transform);
                WeatherParticle.transform.localPosition = Vector3.zero;
                WeatherParticle.transform.localRotation = Quaternion.identity;
                WeatherParticle.transform.localScale = Vector3.one;
            }
            else
            {
                WeatherParticle = null;
            }

            if (transitionDuration > 0)
            {
                IsTransitioning = true;

                Tween.DoFloat(0, 1, transitionDuration, (float t) => {
                    this.t = t;
                }).OnComplete(() => {
                    IsTransitioning = false;
                    OldWeatherPreset = null;
                });
            }
            else
            {

            }
        }

        public void RemoveWeatherPreset(float transitionDuration)
        {
            if (CurrentWeatherPreset == null) return;

            if (transitionDuration > 0)
            {
                OldWeatherPreset = CurrentWeatherPreset;

                IsTransitioning = true;

                Tween.DoFloat(0, 1, transitionDuration, (float t) => {
                    this.t = t;
                }).OnComplete(() => {
                    IsTransitioning = false;
                    OldWeatherPreset = null;
                });

                if (OldWeatherPreset != null && WeatherParticle != null)
                {
                    OldWeatherParticle = WeatherParticle;
                    OldWeatherParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    Tween.DelayedCall(OldWeatherParticle.main.duration, () =>
                    {
                        Object.Destroy(OldWeatherParticle.gameObject);
                    });
                }
            }
            else
            {
                if (WeatherParticle != null)
                {
                    OldWeatherParticle = WeatherParticle;
                    OldWeatherParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    Tween.DelayedCall(OldWeatherParticle.main.duration, () =>
                    {
                        Object.Destroy(OldWeatherParticle.gameObject);
                    });
                }
            }

            CurrentWeatherPreset = null;
        }

        public Color GetLightColor(Color color)
        {
            Color weatherColor = Color.white;
            Color oldWeatherColor = Color.white;

            if (CurrentWeatherPreset != null) weatherColor = CurrentWeatherPreset.GetLightColor(color);
            if (OldWeatherPreset != null) oldWeatherColor = OldWeatherPreset.GetLightColor(color);

            if (CurrentWeatherPreset != null)
            {
                if (!IsTransitioning)
                    return weatherColor;
                else
                {
                    if (OldWeatherPreset == null)
                        return Color.Lerp(color, weatherColor, t);
                    else
                        return Color.Lerp(oldWeatherColor, weatherColor, t);
                }
            }
            else if (OldWeatherPreset != null)
                return Color.Lerp(oldWeatherColor, color, t);

            return color;
        }

        public Color GetSkyColor(Color color, float gradientT)
        {
            Color weatherColor = Color.white;
            Color oldWeatherColor = Color.white;

            if (CurrentWeatherPreset != null) weatherColor = CurrentWeatherPreset.SampleSkyGradient(color, gradientT);
            if (OldWeatherPreset != null) oldWeatherColor = OldWeatherPreset.SampleSkyGradient(color, gradientT);

            if (CurrentWeatherPreset != null)
            {
                if (!IsTransitioning)
                    return weatherColor;
                else
                {
                    if (OldWeatherPreset == null)
                        return Color.Lerp(color, weatherColor, t);
                    else
                        return Color.Lerp(oldWeatherColor, weatherColor, t);
                }
            }
            else if (OldWeatherPreset != null)
                return Color.Lerp(oldWeatherColor, color, t);

            return color;
        }

        public float GetWindMultiplier(float wind)
        {
            float weatherWind = 0f;
            float oldWeatherWind = 0f;

            if (CurrentWeatherPreset != null) weatherWind = CurrentWeatherPreset.GetWindMultiplier(wind);
            if (OldWeatherPreset != null) oldWeatherWind = OldWeatherPreset.GetWindMultiplier(wind);

            return GetFloatValue(wind, oldWeatherWind, weatherWind);
        }

        public float GetShadowsMultiplier(float shadows)
        {
            float weatherShadows = 0f;
            float oldWeatherShadows = 0f;

            if (CurrentWeatherPreset != null) weatherShadows = CurrentWeatherPreset.GetShadowsMultiplier(shadows);
            if (OldWeatherPreset != null) oldWeatherShadows = OldWeatherPreset.GetShadowsMultiplier(shadows);

            return GetFloatValue(shadows, oldWeatherShadows, weatherShadows);
        }

        public float GetRimMultiplier(float rim)
        {
            float weatherRim = 0f;
            float oldWeatherRim = 0f;

            if (CurrentWeatherPreset != null) weatherRim = CurrentWeatherPreset.GetRimMultiplier(rim);
            if (OldWeatherPreset != null) oldWeatherRim = OldWeatherPreset.GetRimMultiplier(rim);

            return GetFloatValue(rim, oldWeatherRim, weatherRim);
        }

        public float GetFloatValue(float value, float oldValue, float currentValue)
        {
            if (CurrentWeatherPreset != null)
            {
                if (!IsTransitioning)
                    return currentValue;
                else
                {
                    if (OldWeatherPreset == null)
                        return Mathf.Lerp(value, currentValue, t);
                    else
                        return Mathf.Lerp(oldValue, currentValue, t);
                }
            }
            else if (OldWeatherPreset != null)
                return Mathf.Lerp(oldValue, value, t);

            return value;
        }

        public DuoFloat GetCloudsInfluence(DuoFloat clouds)
        {
            DuoFloat weatherClouds = 0f;
            DuoFloat oldWeatherClouds = 0f;

            if (CurrentWeatherPreset != null) weatherClouds = CurrentWeatherPreset.GetCloudsInfluence(clouds);
            if (OldWeatherPreset != null) oldWeatherClouds = OldWeatherPreset.GetCloudsInfluence(clouds);

            if (CurrentWeatherPreset != null)
            {
                if (!IsTransitioning)
                    return weatherClouds;
                else
                {
                    if (OldWeatherPreset == null)
                        return DuoFloat.Lerp(clouds, weatherClouds, t);
                    else
                        return DuoFloat.Lerp(oldWeatherClouds, weatherClouds, t);
                }
            }
            else if (OldWeatherPreset != null)
                return DuoFloat.Lerp(oldWeatherClouds, clouds, t);

            return clouds;
        }

        public void ApplyFog()
        {
            if (CurrentWeatherPreset != null)
            {
                if (!IsTransitioning)
                {
                    SetFogSettings(
                        CurrentWeatherPreset.FogEnabled,
                        CurrentWeatherPreset.FogColor,
                        CurrentWeatherPreset.FogDensity,
                        CurrentWeatherPreset.FogMode,
                        CurrentWeatherPreset.FogRange
                        );
                }
                else
                {
                    if (OldWeatherPreset == null)
                    {
                        var newDencity = CurrentWeatherPreset.FogEnabled ? CurrentWeatherPreset.FogDensity : 0;
                        var newRange = CurrentWeatherPreset.FogEnabled ? CurrentWeatherPreset.FogRange : new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1);

                        SetFogSettings(
                            CurrentWeatherPreset.FogEnabled,
                            CurrentWeatherPreset.FogColor,
                            Mathf.Lerp(0, newDencity, t),
                            CurrentWeatherPreset.FogMode,
                            DuoFloat.Lerp(new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1), newRange, t)
                            );
                    }
                    else
                    {

                        var oldDencity = OldWeatherPreset.FogEnabled ? OldWeatherPreset.FogDensity : 0;
                        var newDencity = CurrentWeatherPreset.FogEnabled ? CurrentWeatherPreset.FogDensity : 0;

                        var oldRange = OldWeatherPreset.FogEnabled ? OldWeatherPreset.FogRange : new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1);
                        var newRange = CurrentWeatherPreset.FogEnabled ? CurrentWeatherPreset.FogRange : new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1);

                        SetFogSettings(
                            OldWeatherPreset.FogEnabled || CurrentWeatherPreset.FogEnabled,
                            Color.Lerp(OldWeatherPreset.FogColor, CurrentWeatherPreset.FogColor, t),
                            Mathf.Lerp(oldDencity, newDencity, t),
                            CurrentWeatherPreset.FogMode,
                            DuoFloat.Lerp(oldRange, newRange, t)
                            );
                    }
                }
            }
            else if (OldWeatherPreset != null)
            {
                var oldDencity = OldWeatherPreset.FogEnabled ? OldWeatherPreset.FogDensity : 0;
                var oldRange = OldWeatherPreset.FogEnabled ? OldWeatherPreset.FogRange : new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1);

                SetFogSettings(
                    OldWeatherPreset.FogEnabled,
                    Color.Lerp(OldWeatherPreset.FogColor, OldWeatherPreset.FogColor.SetAlpha(0), t),
                    Mathf.Lerp(oldDencity, 0, t),
                    OldWeatherPreset.FogMode,
                    DuoFloat.Lerp(oldRange, new DuoFloat(Camera.main.farClipPlane + 1, Camera.main.farClipPlane + 1), t)
                    );
            }
            else
            {
                RenderSettings.fog = false;
            }
        }

        private void SetFogSettings(bool fogEnabled, Color fogColor, float fogDensity, FogMode fogMode, DuoFloat fogRange)
        {
            RenderSettings.fog = fogEnabled;

            if (fogEnabled)
            {
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogMode = fogMode;

                if (fogMode == FogMode.Linear)
                {
                    RenderSettings.fogStartDistance = fogRange.firstValue;
                    RenderSettings.fogEndDistance = fogRange.secondValue;
                }
            }
        }
    }
}
