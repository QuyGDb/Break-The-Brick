using UnityEngine;

namespace Watermelon
{
    public class AudioSourceCase
    {
        private AudioSource audioSource;
        public AudioSource AudioSource => audioSource;

        public bool IsPlaying => audioSource.isPlaying;

        private AudioType audioType;
        private float clipVolume;

        private GameObject gameObject;
        public GameObject GameObject => gameObject;

        public AudioSourceCase()
        {
            gameObject = new GameObject("[AUDIO SOURCE OBJECT]");

            GameObject.DontDestroyOnLoad(gameObject);

            //gameObject.hideFlags = HideFlags.HideInInspector;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            AudioController.ApplyDefaultSettings(ref audioSource);
        }

        public void Play(AudioClip audioClip, float clipVolume, AudioType type = AudioType.Sound)
        {
            audioType = type;

            audioSource.clip = audioClip;
            audioSource.volume = clipVolume * AudioController.GetVolume(audioType);

            audioSource.Play();
        }

        public void OverrideVolume(AudioType type, float volume)
        {
            if (!audioSource.isPlaying || audioType != type) return;

            audioSource.volume = volume * clipVolume;
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