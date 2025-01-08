using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Gamepad Data", menuName = "Core/Gamepad Data")]
    public class GamepadData : ScriptableObject
    {
        [SerializeField] List<ButtonData> data;

        private Dictionary<GamepadButtonType, Sprite> iconsDictionary;

        public void Initialise()
        {
            iconsDictionary = new Dictionary<GamepadButtonType, Sprite>();

            for(int i = 0; i < data.Count; i++)
            {
                iconsDictionary.Add(data[i].button, data[i].icon);
            }
        }

        public Sprite GetButtonIcon(GamepadButtonType button)
        {
            return iconsDictionary[button];
        }

        [System.Serializable]
        public class ButtonData
        {
            public GamepadButtonType button;
            public Sprite icon;
        }
    }
}