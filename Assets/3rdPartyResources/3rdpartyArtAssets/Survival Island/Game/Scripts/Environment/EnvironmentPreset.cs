using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Environment Preset", menuName = "Content/Weather/Environment Preset")]
    public class EnvironmentPreset : ScriptableObject
    {
        [SerializeField] EnvironmentPresetType type;
        public EnvironmentPresetType Type => type;

        [OnValueChanged("ValidatePartOfDay")]
        [SerializeField] PartOfDay enabledPartsOfDay;
        public PartOfDay EnabledPartsOfDay => enabledPartsOfDay;

        [SerializeField] float dayPartTransitionDuration;
        public float DayPartTransitionDuration => dayPartTransitionDuration;

        [SerializeField] WeatherContainer[] weather;
        public WeatherContainer[] Weather => weather;

        [SerializeField] float weatherTransitionDuration;
        public float WeatherTransitionDuration => weatherTransitionDuration;

        [Tab("Day", showIf: "DayEnabled"), UnpackNested]
        [SerializeField] PartOfDayPreset dayPreset;
        [Tab("Night", showIf: "NightEnabled"), UnpackNested]
        [SerializeField] PartOfDayPreset nightPreset;
        [Tab("Morning", showIf: "MorningEnabled"), UnpackNested]
        [SerializeField] PartOfDayPreset morningPreset;
        [Tab("Evening", showIf: "EveningEnabled"), UnpackNested]
        [SerializeField] PartOfDayPreset eveningPreset;

        public PartOfDayPreset DayPreset => dayPreset;
        public PartOfDayPreset NightPreset => nightPreset;
        public PartOfDayPreset MorningPreset => morningPreset;
        public PartOfDayPreset EveningPreset => eveningPreset;

        private bool DayEnabled() => (enabledPartsOfDay & PartOfDay.Day) == PartOfDay.Day;
        private bool NightEnabled() => (enabledPartsOfDay & PartOfDay.Night) == PartOfDay.Night;
        private bool MorningEnabled() => (enabledPartsOfDay & PartOfDay.Morning) == PartOfDay.Morning;
        private bool EveningEnabled() => (enabledPartsOfDay & PartOfDay.Evening) == PartOfDay.Evening;

        [Button("Apply Morning", "MorningEnabled", ButtonVisibility.ShowIf), HorizontalGroup("Apply Preset")]
        public void ApplyMorningPreset() => ApplyPreset(PartOfDay.Morning);

        [Button("Apply Day", "DayEnabled", ButtonVisibility.ShowIf), HorizontalGroup("Apply Preset")]
        public void ApplyDayPreset() => ApplyPreset(PartOfDay.Day);

        [Button("Apply Evening", "EveningEnabled", ButtonVisibility.ShowIf), HorizontalGroup("Apply Preset")]
        public void ApplEveningPreset() => ApplyPreset(PartOfDay.Evening);

        [Button("Apply Night", "NightEnabled", ButtonVisibility.ShowIf), HorizontalGroup("Apply Preset")]
        public void ApplyNightPreset() => ApplyPreset(PartOfDay.Night);

        private void ApplyPreset(PartOfDay partOfDay)
        {
            if (!Application.isPlaying) return;

            EnvironmentController.ApplyPresetDev(this, partOfDay);
        }

        private void ValidatePartOfDay()
        {
            if(enabledPartsOfDay == 0)
            {
                enabledPartsOfDay = PartOfDay.Day;
            }
        }
    }

    [System.Serializable]
    public class PartOfDayPreset
    {
        [Header("Time")]
        [SerializeField] float partOfDayDuration = 60;
        public float PartOfDayDuration => partOfDayDuration;

        [Header("Light")]
        [SerializeField] Color lightColor = Color.white;
        public Color LightColor => lightColor;

        [Header("Shadows")]
        [SerializeField, Range(0, 2)] float shadowsIntensity = 1;
        public float ShadowsIntensity => shadowsIntensity;

        [Header("Rim")]
        [SerializeField, Range(0, 2)] float rimIntensity = 1;
        public float RimIntensity => rimIntensity;

        [Header("Sky")]
        [SerializeField] Gradient skyGradient;
        public Gradient SkyGradient => skyGradient;

        [Header("Wind")]
        [SerializeField, Range(0, 10)] float windMultiplier = 1;
        public float WindMultiplier => windMultiplier;

        [Header("Clouds")]
        [SerializeField, Range(0, 1)] float cloudsInfluence1 = 1;
        public float CloudsInfluence1 => cloudsInfluence1;
        [SerializeField, Range(0, 1)] float cloudsInfluence2 = 1;
        public float CloudsInfluence2 => cloudsInfluence2;
    }

    [System.Serializable]
    public class WeatherContainer
    {
        [SerializeField] EnvironmentWeatherPreset preset;
        public EnvironmentWeatherPreset WeatherPreset => preset;

        [SerializeField] DuoFloat duration;
        public DuoFloat Duration => duration;

        [SerializeField, Range(0, 100)] float chance;
        public float Chance => chance;
    }
}