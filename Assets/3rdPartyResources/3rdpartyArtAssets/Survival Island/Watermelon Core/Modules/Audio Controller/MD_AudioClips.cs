using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("UI")]
        public AudioClip reward;

        [BoxGroup("Resources", "Resources")]
        public AudioClip resourcesDropInStorageSound;

        [BoxGroup("Resources")]
        public AudioClip resourcesPickUpFromStorageSound;

        [BoxGroup("Movement", "Movement")]
        public AudioClip waterSound;
        [BoxGroup("Movement")]
        public List<AudioClip> stepSounds;

        [BoxGroup("General","General")]
        public AudioClip appear;
        
        [BoxGroup("General")]
        public AudioClip boost;
        
        [BoxGroup("General")]
        public AudioClip hummer;

        [BoxGroup("Combat", "Combat")]
        public AudioClip punch;
        [BoxGroup("Combat")]
        public AudioClip playerDeathSound;
        [BoxGroup("Combat")]
        public AudioClip playerDrowningSound;

    }
}

// -----------------
// Audio Controller v 0.4
// -----------------