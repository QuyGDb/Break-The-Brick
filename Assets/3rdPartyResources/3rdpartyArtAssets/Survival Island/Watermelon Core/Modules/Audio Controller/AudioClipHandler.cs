using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class AudioClipHandler
    {
        [SerializeField] AudioType audioType;

        [Slider(0.0f, 1.0f)]
        [SerializeField] float clipVolume = 1.0f;

        [SerializeField] bool advancedSettings;

        [ShowIf("advancedSettings")]
        [SerializeField] float minDelay;

        [ShowIf("advancedSettings")]
        [SerializeField] bool dynamicPitch;
        [ShowIf("advancedSettings")]
        [SerializeField] DuoFloat pitchRange = new DuoFloat(0.8f, 1.2f);
        [ShowIf("advancedSettings")]
        [SerializeField] int pitchSteps = 10;
        [ShowIf("advancedSettings")]
        [SerializeField] float pitchResetTime = 1.0f;

        [ShowIf("advancedSettings")]
        [SerializeField] AudioSource customAudioSource;

        [System.NonSerialized]
        private float lastPlayedTime = float.MinValue;

        private int currentPitchStep;
        private float lastPitchStepTime;

        private TweenCase volumeCase;

        public void Initialise()
        {
            AudioController.VolumeChanged += OnVolumeChanged;

            lastPlayedTime = float.MinValue;
        }

        public void Unload()
        {
            AudioController.VolumeChanged -= OnVolumeChanged;

            volumeCase.KillActive();
        }

        private void OnVolumeChanged(AudioType type, float volume)
        {
            if (!advancedSettings) return;
            if (customAudioSource == null) return;
            if (audioType != type) return;

            customAudioSource.volume = volume * clipVolume;
        }

        public void Play(AudioClip audioClip)
        {
            float pitch = 1.0f;

            if(advancedSettings)
            {
                if (Time.time < lastPlayedTime + minDelay) return;

                lastPlayedTime = Time.time;

                if(dynamicPitch)
                {
                    currentPitchStep++;

                    if(Time.time > lastPitchStepTime)
                        currentPitchStep = 0;

                    lastPitchStepTime = Time.time + pitchResetTime;

                    pitch = pitchRange.Lerp((float)currentPitchStep / pitchSteps);
                }

                if(customAudioSource != null)
                {
                    customAudioSource.clip = audioClip;
                    customAudioSource.volume = AudioController.GetVolume(audioType) * clipVolume;
                    customAudioSource.pitch = pitch;

                    customAudioSource.Play();

                    return;
                }
            }

            AudioController.PlaySound(audioClip, clipVolume, pitch);
        }

        public void Play(AudioClip audioClip, Vector3 position)
        {
            float pitch = 1.0f;

            if (advancedSettings)
            {
                if (Time.time < lastPlayedTime + minDelay) return;

                lastPlayedTime = Time.time;

                if (dynamicPitch)
                {
                    currentPitchStep++;

                    if (Time.time > lastPitchStepTime)
                        currentPitchStep = 0;

                    lastPitchStepTime = Time.time + pitchResetTime;

                    pitch = pitchRange.Lerp((float)currentPitchStep / pitchSteps);
                }

                if (customAudioSource != null)
                {
                    customAudioSource.clip = audioClip;
                    customAudioSource.volume = AudioController.GetVolume(audioType) * clipVolume;
                    customAudioSource.pitch = pitch;

                    customAudioSource.Play();

                    return;
                }
            }

            AudioController.PlaySound(audioClip, position, clipVolume, pitch);
        }

        public TweenCase DoVolume(float volume, float duration)
        {
            if (advancedSettings && customAudioSource != null)
            {
                volumeCase.KillActive();
                volumeCase = Tween.DoFloat(clipVolume, volume, duration, (value) =>
                {
                    clipVolume = value;
                    customAudioSource.volume = AudioController.GetVolume(audioType) * clipVolume;
                });

                return volumeCase;
            }

            return null;
        }

        public void StopPlaying()
        {
            if(advancedSettings && customAudioSource != null)
            {
                customAudioSource.Stop();
            }
        }
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------

// Changelog
// v 0.4
// • Vibration settings removed
// v 0.3.3
// • Method for separate music and sound volume override
// v 0.3.2
// • Added audio listener creation method
// v 0.3.2
// • Added volume float
// • AudioSettings variable removed (now sounds, music and vibrations can be reached directly)
// v 0.3.1
// • Added OnVolumeChanged callback
// • Renamed AudioSettings to Settings
// v 0.3
// • Added IsAudioModuleEnabled method
// • Added IsVibrationModuleEnabled method
// • Removed VibrationToggleButton class
// v 0.2
// • Removed MODULE_VIBRATION
// v 0.1
// • Added basic version
// • Added support of new initialization
// • Music and Sound volume is combined