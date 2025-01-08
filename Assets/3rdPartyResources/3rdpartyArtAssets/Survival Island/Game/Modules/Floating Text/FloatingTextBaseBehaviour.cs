using TMPro;
using UnityEngine;

namespace Watermelon
{
    public abstract class FloatingTextBaseBehaviour : MonoBehaviour
    {
        [SerializeField] protected TMP_Text textRef;

        public virtual void Activate(string text, Color color)
        {
            textRef.text = text;
            textRef.color = color;
        }
    }
}