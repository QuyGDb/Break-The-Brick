using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FirstAnimationEnabler : MonoBehaviour
    {
        [SerializeField, UniqueID] string id;

        public void Start()
        {
            SimpleBoolSave alreadyPlayedAnimationSave = SaveController.GetSaveObject<SimpleBoolSave>("FirstAnimation" + id);

            if (!alreadyPlayedAnimationSave.Value)
            {
                Control.DisableMovementControl();
                PlayerBehavior.GetBehavior().PlayerGraphics.RunWakeUpAnimation();

                alreadyPlayedAnimationSave.Value = true;
                SaveController.MarkAsSaveIsRequired();

                Tween.DelayedCall(3f, () =>
                {
                    Control.EnableMovementControl();
                });
            }
        }
    }
}
