using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public class UIStore : UIPage
    {
        [SerializeField] VerticalLayoutGroup layout;
        [SerializeField] RectTransform safeAreaTransform;
        [SerializeField] RectTransform content;
        [SerializeField] Button closeButton;
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        private TweenCase[] appearTweenCases;
        private Transform[] offersTransforms;

        private void Awake()
        {
            offersTransforms = new Transform[content.childCount];
            for(int i = 0; i < offersTransforms.Length; i++)
            {
                offersTransforms[i] = content.GetChild(i);
            }

            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public override void Initialise()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaTransform);

            coinsUI.Initialise();
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            appearTweenCases.KillActive();

            appearTweenCases = new TweenCase[offersTransforms.Length];
            for (int i = 0; i < offersTransforms.Length; i++)
            {
                Transform offerTransform = offersTransforms[i].transform;
                offerTransform.transform.localScale = Vector3.zero;

                appearTweenCases[i] = offerTransform.transform.DOScale(1.0f, 0.3f, i * 0.05f).SetEasing(Ease.Type.CircOut);
            }

            closeButton.transform.localScale = Vector3.zero;
            closeButton.transform.DOScale(1.0f, 0.3f, 0.2f).SetEasing(Ease.Type.BackOut);

            content.anchoredPosition = Vector2.zero;

            appearTweenCases[^1].OnComplete(() =>
            {
                UIController.OnPageOpened(this);
            });
        }

        public void Hide()
        {
            appearTweenCases.KillActive();

            UIController.HidePage<UIStore>();
        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIStore>();
        }

        public void SpawnCurrencyCloud(RectTransform spawnRectTransform, CurrencyType currencyType, int amount, SimpleCallback completeCallback = null)
        {
            FloatingCloud.SpawnCurrency(currencyType.ToString(), spawnRectTransform, coinsUI.RectTransform, amount, null, completeCallback);
        }
    }
}

// -----------------
// IAP Store v1.2
// -----------------

// Changelog
// v 1.2
// • Added mobile notch offset support
// • Added free timer money offer
// • Added ad money offer
// v 1.1
// • Added offers interface
// • Offers prefabs renamed
// v 1.0
// • Basic logic