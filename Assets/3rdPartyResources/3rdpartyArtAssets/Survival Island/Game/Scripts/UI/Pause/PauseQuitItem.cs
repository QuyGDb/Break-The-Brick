using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class PauseQuitItem : PauseItem
    {
        protected override void Awake()
        {
            base.Awake();
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Destroy(this.gameObject);
#endif
        }

        protected override void Click()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}