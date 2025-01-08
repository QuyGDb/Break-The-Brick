#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Tween", true)]
    public class TweenInitModule : InitModule
    {
        public override string ModuleName => "Tween";

        [SerializeField] CustomEasingFunction[] customEasingFunctions;

        [Space]
        [SerializeField] int tweensUpdateCount = 300;
        [SerializeField] int tweensFixedUpdateCount = 30;
        [SerializeField] int tweensLateUpdateCount = 0;

        [Space]
        [SerializeField] bool verboseLogging;

        public override void CreateComponent(GameObject holderObject)
        {
            Tween tween = holderObject.AddComponent<Tween>();
            tween.Initialise(tweensUpdateCount, tweensFixedUpdateCount, tweensLateUpdateCount, verboseLogging);

            Ease.Initialise(customEasingFunctions);
        }
    }
}
