using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Floating Message", Core = true)]
    public class FloatingMessageInitModule : InitModule
    {
        public override string ModuleName => "Floating Message";

        [SerializeField] GameObject canvas;

        public override void CreateComponent(GameObject holderObject)
        {
            GameObject canvasGameObject = Instantiate(canvas);
            canvasGameObject.transform.SetParent(holderObject.transform);
            canvasGameObject.transform.localScale = Vector3.one;
            canvasGameObject.transform.localPosition = Vector3.zero;
            canvasGameObject.transform.localRotation = Quaternion.identity;
            canvasGameObject.GetComponent<FloatingMessage>().Initialise();
        }
    }
}

// -----------------
// Floating Message v 0.1
// -----------------