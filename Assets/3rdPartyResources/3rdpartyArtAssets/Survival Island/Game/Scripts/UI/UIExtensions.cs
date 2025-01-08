using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public static class UIExtensions
    {
        public static void ClickButton(this Button button)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);

            Tween.NextFrame(() =>
            {
                var eventData = new PointerEventData(EventSystem.current);

                eventData.button = PointerEventData.InputButton.Left;
                button.OnPointerClick(eventData);

                Tween.DelayedCall(0.2f, () => EventSystem.current.SetSelectedGameObject(null));
            });
        }
    }
}
