namespace Watermelon
{
    [System.Serializable]
    public class AudioSave : ISaveObject
    {
        public VolumeData[] VolumeDatas;

        public void Flush()
        {
            AudioType[] audioTypes = EnumUtils.GetEnumArray<AudioType>();

            VolumeDatas = new VolumeData[audioTypes.Length];

            for (int i = 0; i < audioTypes.Length; i++)
            {
                VolumeDatas[i] = new VolumeData() { AudioType = audioTypes[i], Volume = AudioController.GetVolume(audioTypes[i]) };
            }
        }

        [System.Serializable]
        public class VolumeData
        {
            public AudioType AudioType;
            public float Volume;
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