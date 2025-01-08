using UnityEngine;

namespace Watermelon
{
    public class TutorialArrowBehaviour : MonoBehaviour
    {
        public void Activate()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(1.0f, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void Disable()
        {
            transform.DOScale(0.0f, 0.2f).SetEasing(Ease.Type.BackIn).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}