using UnityEngine;

namespace Watermelon
{
    public static class Haptic
    {
        public static readonly HapticData HAPTIC_LIGHT = new HapticData(0.05f, 0.0f);
        public static readonly HapticData HAPTIC_MEDIUM = new HapticData(0.08f, 0.4f);
        public static readonly HapticData HAPTIC_HARD = new HapticData(0.12f, 0.6f);

        public static readonly HapticPattern PATTERN_LIGHT = new HapticPattern("light", new HapticEvent[] { new HapticEvent() { Duration = 0.3f, Intensity = 1.0f, Sharpness = 0.0f, StartTime = 0.0f } });

        private static bool isActive;
        public static bool IsActive 
        { 
            get { return isActive; }
            set 
            { 
                isActive = value;

                save.Value = value;

                StateChanged?.Invoke(value);
            }
        }

        public static bool IsInitialised { get; private set; }
        public static bool VerboseLogging { get; private set; }

        private static readonly BaseHapticWrapper WRAPPER = GetPlatformWrapper();

        private static SimpleBoolSave save;

        public static event SimpleBoolCallback StateChanged;

        public static void Initialise()
        {
            // Get saved state
            save = SaveController.GetSaveObject<SimpleBoolSave>("haptic");

            // Set saved state
            isActive = save.Value;

            if(WRAPPER == null)
            {
                Debug.LogWarning("[Haptic]: Unsupported platform");

                return;
            }

            // Mark as initialised
            IsInitialised = true;

            // Initialise platform handler
            WRAPPER.Initialise();

            // Register default patterns
            WRAPPER.RegisterPattern(PATTERN_LIGHT);
        }

        public static void RegisterPattern(HapticPattern hapticPattern)
        {
            if (WRAPPER == null) return;

            WRAPPER.RegisterPattern(hapticPattern);
        }

        public static void Play(HapticData hapticData)
        {
            Play(hapticData.Duration, hapticData.Intensity);
        }

        public static void Play(float duration, float intensity = 1.0f)
        {
            if (!IsActive) return;

            if (WRAPPER == null) return;

            if (duration <= 0) return;

            WRAPPER.Play(duration, intensity);
        }

        public static void Play(HapticPattern pattern)
        {
            if (!IsActive) return;

            if (WRAPPER == null) return;

            WRAPPER.Play(pattern.ID);
        }

        public static void Play(string patternID)
        {
            if (!IsActive) return;

            if (WRAPPER == null) return;

            WRAPPER.Play(patternID);
        }

        public static void EnableVerboseLogging()
        {
            VerboseLogging = true;
        }

        private static BaseHapticWrapper GetPlatformWrapper()
        {
#if UNITY_EDITOR
            return new EditorHapticWrapper();
#elif UNITY_IOS
            return new IOSHapticWrapper();
#elif UNITY_ANDROID
            return new AndroidHapticWrapper();
#elif UNITY_WEBGL
            return new WebGLHapticWrapper();
#else
            return null;
#endif
        }
    }
}
