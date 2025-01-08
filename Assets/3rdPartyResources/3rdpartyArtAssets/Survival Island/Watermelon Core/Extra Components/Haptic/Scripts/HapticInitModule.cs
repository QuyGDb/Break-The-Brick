using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Haptic")]
    public class HapticInitModule : InitModule
    {
        [SerializeField] bool verboseLogging = false;

        public override string ModuleName => "Haptic";

        public override void CreateComponent(GameObject holderObject)
        {
            if (verboseLogging)
                Haptic.EnableVerboseLogging();

            Haptic.Initialise();
        }
    }
}
