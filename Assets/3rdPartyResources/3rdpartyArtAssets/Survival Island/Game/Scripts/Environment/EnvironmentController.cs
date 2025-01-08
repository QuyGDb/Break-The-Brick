using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class EnvironmentController : MonoBehaviour
    {
        private static readonly int _WeatherWindMultiplier = Shader.PropertyToID("_WeatherWindMultiplier");

        private static readonly int _CloudsInfluence1 = Shader.PropertyToID("_CloudsInfluence1");
        private static readonly int _CloudsInfluence2 = Shader.PropertyToID("_CloudsInfluence2");

        private static readonly int _ShadowsIntensity = Shader.PropertyToID("_ShadowsIntensity");
        private static readonly int _RimIntensity = Shader.PropertyToID("_RimIntensity");

        private static EnvironmentController instance;

        private static List<WeatherContainer> weather = new List<WeatherContainer>();

        private static List<Material> cachedMaterials = new List<Material>();

        [SerializeField] EnvironmentPresetsDatabase database;
        public static EnvironmentPresetsDatabase Database => instance.database;

        public static EnvironmentPreset CurrentPreset { get; private set; }
        public static List<PartOfDayPreset> PartsOfDayPresets { get; private set; }
        public static PartOfDayPreset CurrentPartOfDay { get; private set; }

        private static Light Light { get; set; }

        private static Coroutine daynightCoroutine;
        private static Coroutine weatherCoroutine;

        [SerializeField] bool dayNightEnabled = true;
        public static bool DayNightEnabled { get => instance.dayNightEnabled; set => instance.dayNightEnabled = value; }

        [SerializeField] bool weatherEnabled = true;
        public static bool WeatherEnabled { get => instance.weatherEnabled; set => instance.weatherEnabled = value; }

        [SerializeField, Tooltip("Updates preset parameters every frame")] bool debug = false;
        public static bool IsDebug { get => instance.debug; set => instance.debug = value; }

        private static EnvironmentSkyModule skyModule;
        private static EnvironmentWeatherModule weatherModule;

        public static bool TransitionInProgress { get; private set; }

        public void Initialise()
        {
            instance = this;

            if (database == null)
            {
                Destroy(this);
                return;
            }

            weatherModule = new EnvironmentWeatherModule();
            skyModule = new EnvironmentSkyModule(weatherModule);
        }

        private void Update()
        {
            // Update for debug or weather transition
            if (!TransitionInProgress && (IsDebug || weatherModule.IsTransitioning))
            {
                ApplyDayPartPreset(CurrentPartOfDay);
            }
        }

        private static IEnumerator DayNightCoroutine()
        {
            int i = 0;
            CurrentPartOfDay = PartsOfDayPresets[i];

            var pause = new WaitUntil(() => DayNightEnabled);

            while (true)
            {
                yield return new WaitForSeconds(CurrentPartOfDay.PartOfDayDuration);
                if (!DayNightEnabled) yield return pause;

                var nextPartOfDay = PartsOfDayPresets[++i % PartsOfDayPresets.Count];
                yield return PartsOfDayTransition(CurrentPartOfDay, nextPartOfDay);

                CurrentPartOfDay = nextPartOfDay;
            }
        }

        public static IEnumerator PartsOfDayTransition(PartOfDayPreset from, PartOfDayPreset to)
        {
            TransitionInProgress = true;

            var time = 0f;
            var duration = from.PartOfDayDuration;

            var pause = new WaitUntil(() => DayNightEnabled);

            while (time < duration)
            {
                time += Time.deltaTime;
                var t = time / duration;

                // LIGHT COLOR
                var environmentLightColor = Color.Lerp(from.LightColor, to.LightColor, t);
               if(Light != null) Light.color = weatherModule.GetLightColor(environmentLightColor);

                // SKY
                skyModule.LerpDayPartPresets(from, to, t);

                // WIND
                var environmentWind = Mathf.Lerp(from.WindMultiplier, to.WindMultiplier, t);
                var weatherWinds = weatherModule.GetWindMultiplier(environmentWind);
                Shader.SetGlobalFloat(_WeatherWindMultiplier, weatherWinds);

                // SHADOWS
                var environmentShadows = Mathf.Lerp(from.ShadowsIntensity, to.ShadowsIntensity, t);
                var weatherShadows = weatherModule.GetShadowsMultiplier(environmentShadows);
                Shader.SetGlobalFloat(_ShadowsIntensity, weatherShadows);

                // RIM
                var environmentRim = Mathf.Lerp(from.RimIntensity, to.RimIntensity, t);
                var weatherRim = weatherModule.GetShadowsMultiplier(environmentRim);
                Shader.SetGlobalFloat(_RimIntensity, weatherRim);

                // CLOUDS
                if (Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
                {
                    var environmentClouds1 = Mathf.Lerp(from.CloudsInfluence1, to.CloudsInfluence1, t);
                    var environmentClouds2 = Mathf.Lerp(from.CloudsInfluence2, to.CloudsInfluence2, t);

                    var environmentCloudsInfluence = new DuoFloat(environmentClouds1, environmentClouds2);
                    var weatherCloudsInfluence = weatherModule.GetCloudsInfluence(environmentCloudsInfluence);

                    cloudsTexture.material.SetFloat(_CloudsInfluence1, weatherCloudsInfluence.firstValue);
                    cloudsTexture.material.SetFloat(_CloudsInfluence2, weatherCloudsInfluence.secondValue);
                }

                weatherModule.ApplyFog();

                yield return null;
                if (!DayNightEnabled) yield return pause;
            }

            ApplyDayPartPreset(to);

            TransitionInProgress = false;
        }

        private static void ApplyDayPartPreset(PartOfDayPreset preset)
        {
            if (Light != null) Light.color = weatherModule.GetLightColor(preset.LightColor);

            skyModule.ApplyDayPartPreset(preset);

            Shader.SetGlobalFloat(_WeatherWindMultiplier, weatherModule.GetWindMultiplier(preset.WindMultiplier));
            Shader.SetGlobalFloat(_ShadowsIntensity, weatherModule.GetShadowsMultiplier(preset.ShadowsIntensity));
            Shader.SetGlobalFloat(_RimIntensity, weatherModule.GetShadowsMultiplier(preset.RimIntensity));

            if (Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
            {
                DuoFloat cloudsInfluence = new DuoFloat(preset.CloudsInfluence1, preset.CloudsInfluence2);
                cloudsInfluence = weatherModule.GetCloudsInfluence(cloudsInfluence);

                cloudsTexture.material.SetFloat(_CloudsInfluence1, cloudsInfluence.firstValue);
                cloudsTexture.material.SetFloat(_CloudsInfluence2, cloudsInfluence.secondValue);
            }

            weatherModule.ApplyFog();
        }

        public static void SetPreset(EnvironmentPresetType type)
        {
            Light = FindObjectOfType<Light>();

            if(Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
            {
                if(!cachedMaterials.Contains(cloudsTexture.material)) cachedMaterials.Add(cloudsTexture.material);
            }

            CurrentPreset = Database.GetPreset(type);

            PartsOfDayPresets = new List<PartOfDayPreset>();

            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Day) == PartOfDay.Day) PartsOfDayPresets.Add(CurrentPreset.DayPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Evening) == PartOfDay.Evening) PartsOfDayPresets.Add(CurrentPreset.EveningPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Night) == PartOfDay.Night) PartsOfDayPresets.Add(CurrentPreset.NightPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Morning) == PartOfDay.Morning) PartsOfDayPresets.Add(CurrentPreset.MorningPreset);

            CurrentPartOfDay = PartsOfDayPresets[0];

            ApplyDayPartPreset(CurrentPartOfDay);

            if (daynightCoroutine != null) instance.StopCoroutine(daynightCoroutine);

            if (PartsOfDayPresets.Count > 1)
            {
                daynightCoroutine = instance.StartCoroutine(DayNightCoroutine());
            }

            weather.Clear();

            if (!CurrentPreset.Weather.IsNullOrEmpty())
            {
                foreach (var weatherContainer in CurrentPreset.Weather)
                {
                    weather.Add(weatherContainer);
                }
            }

            if (weatherCoroutine != null) instance.StopCoroutine(weatherCoroutine);

            if (weather.Count > 1)
            {
                weatherCoroutine = instance.StartCoroutine(WeatherCoroutine());
            }
            else if (weather.Count == 1)
            {
                weatherModule.SetWeatherPreset(weather[0].WeatherPreset, CurrentPreset.WeatherTransitionDuration);
            }
            else
            {
                weatherModule.RemoveWeatherPreset(0);
            }
        }

        public static void OnWorldUnloaded()
        {
            if (daynightCoroutine != null) instance.StopCoroutine(daynightCoroutine);
            if (weatherCoroutine != null) instance.StopCoroutine(weatherCoroutine);
        }

        private static IEnumerator WeatherCoroutine()
        {
            var pause = new WaitUntil(() => WeatherEnabled);

            while (true)
            {
                var weather = GetNextWeather();

                weatherModule.SetWeatherPreset(weather.WeatherPreset, CurrentPreset.WeatherTransitionDuration);

                yield return new WaitForSeconds(weather.Duration.Random());

                if (!WeatherEnabled) yield return pause;
            }
        }

        public static WeatherContainer GetNextWeather()
        {
            var chanceSum = 0f;
            for (int i = 0; i < weather.Count; i++)
            {
                chanceSum += weather[i].Chance;
            }

            var random = Random.value * chanceSum;

            var counter = 0f;
            for (int i = 0; i < weather.Count; i++)
            {
                var weatherContainer = weather[i];
                counter += weather[i].Chance;

                if (random <= counter) return weather[i];
            }

            return null;
        }

        [Button("Turn On Day")]
        public static void StartDayPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Day) != PartOfDay.Day) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.DayPreset;

            ApplyDayPartPreset(CurrentPreset.DayPreset);
        }

        [Button("Turn On Evening")]
        public static void StartEveningPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Evening) != PartOfDay.Evening) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.EveningPreset;

            ApplyDayPartPreset(CurrentPreset.EveningPreset);
        }

        [Button("Turn On Night")]
        public static void StartNightPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Night) != PartOfDay.Night) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.NightPreset;

            ApplyDayPartPreset(CurrentPreset.NightPreset);
        }

        [Button("Turn On Morning")]
        public static void StartMorningPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Morning) != PartOfDay.Morning) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.MorningPreset;

            ApplyDayPartPreset(CurrentPreset.MorningPreset);
        }

        public static void ApplyPresetDev(EnvironmentPreset preset, PartOfDay partOfDay)
        {
            if(CurrentPreset != preset)
            {
                SetPreset(preset.Type);
            }

            DayNightEnabled = false;
            WeatherEnabled = false;

            if (partOfDay == PartOfDay.Day) CurrentPartOfDay = CurrentPreset.DayPreset;
            if (partOfDay == PartOfDay.Evening) CurrentPartOfDay = CurrentPreset.EveningPreset;
            if (partOfDay == PartOfDay.Night) CurrentPartOfDay = CurrentPreset.NightPreset;
            if (partOfDay == PartOfDay.Morning) CurrentPartOfDay = CurrentPreset.MorningPreset;

            weatherModule.RemoveWeatherPreset(0);

            ApplyDayPartPreset(CurrentPartOfDay);
        }

        public static void ApplyWeatherDev(EnvironmentWeatherPreset preset)
        {
            WeatherEnabled = false;
            weatherModule.SetWeatherPreset(preset, 0);

            ApplyDayPartPreset(CurrentPartOfDay);
        }

        private void OnDestroy()
        {
            for(int i = 0; i < cachedMaterials.Count; i++)
            {
                var material = cachedMaterials[i];

                material.SetFloat(_CloudsInfluence1, 0);
                material.SetFloat(_CloudsInfluence2, 0);
            }
        }
    }
}