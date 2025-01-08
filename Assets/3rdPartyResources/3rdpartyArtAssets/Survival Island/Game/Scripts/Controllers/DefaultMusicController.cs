using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(MusicSource))]
    public class DefaultMusicController : MonoBehaviour
    {
        public static MusicSource MusicSource { get; private set; }

        public void Initialise()
        {
            MusicSource = GetComponent<MusicSource>();
            MusicSource.Initialise();
        }

        public static void ActivateMusic()
        {
            MusicSource.Activate();
            MusicSource.SetAsDefault();
        }
    }
}
