using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [System.Serializable]
    public class SimpleEmoteBehavior
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image emoteImage;

        [Space]
        [SerializeField] EmoteData[] emotes;
        [SerializeField] Sprite defaultEmoteIcon;

        private RectTransform panelRectTransform;

        private bool isActive;
        private EmoteType lastEmote;

        private TweenCase scaleTweenCase;

        private Transform parentTransform;
        private Vector3 defaultOffset;

        private Quaternion defaultRotate;
        private Vector3 defaultScale;

        public void Initialise()
        {
            panelRectTransform = (RectTransform)canvasGroup.transform;
            panelRectTransform.gameObject.SetActive(false);

            parentTransform = panelRectTransform.parent;

            defaultScale = panelRectTransform.localScale;
            defaultOffset = parentTransform.position - panelRectTransform.position;
            defaultRotate = Quaternion.Euler(panelRectTransform.localEulerAngles.x, 0, 0);

            panelRectTransform.SetParent(null);
            panelRectTransform.rotation = defaultRotate;

            isActive = false;
        }

        public void Unload()
        {
            scaleTweenCase.KillActive();
        }

        public void Update()
        {
            if(isActive)
                panelRectTransform.position = parentTransform.position - defaultOffset;
        }

        public void Show(EmoteType emoteType)
        {
            if (isActive && emoteType == lastEmote) return;

            EmoteData emoteData = GetEmote(emoteType);
            if(emoteData != null)
            {
                emoteImage.sprite = emoteData.Icon;
            }
            else
            {
                emoteImage.sprite = defaultEmoteIcon;
            }

            lastEmote = emoteType;

            scaleTweenCase.KillActive();

            panelRectTransform.position = parentTransform.position - defaultOffset;

            panelRectTransform.localScale = Vector3.zero;
            panelRectTransform.gameObject.SetActive(true);

            scaleTweenCase = panelRectTransform.DOScale(defaultScale, 0.3f).SetEasing(Ease.Type.BackOut);

            isActive = true;
        }

        public void Hide()
        {
            if (!isActive) return;

            isActive = false;

            scaleTweenCase.KillActive();

            panelRectTransform.gameObject.SetActive(false);
        }

        private EmoteData GetEmote(EmoteType emoteType)
        {
            for(int i = 0; i < emotes.Length; i++)
            {
                if (emotes[i].EmoteType == emoteType)
                    return emotes[i];
            }

            return null;
        }

        [System.Serializable]
        public class EmoteData
        {
            [SerializeField] EmoteType emoteType;
            public EmoteType EmoteType => emoteType;

            [SerializeField] Sprite icon;
            public Sprite Icon => icon;
        }

        public enum EmoteType
        {
            StorageIsFull = 0, 
            Hunger = 1
        }
    }
}