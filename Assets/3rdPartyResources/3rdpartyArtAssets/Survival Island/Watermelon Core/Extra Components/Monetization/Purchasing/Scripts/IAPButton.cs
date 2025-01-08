using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class IAPButton : MonoBehaviour
    {
        [SerializeField] Image backImage;
        [SerializeField] Button button;
        [SerializeField] TMP_Text priceText;
        [SerializeField] GameObject loadingObject;

        [Space]
        [SerializeField] Sprite activeBackSprite;
        [SerializeField] Sprite unactiveBackSprite;

        private ProductKeyType key;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Initialise(ProductKeyType key)
        {
            this.key = key;

            UpdateState();
        }

        public void UpdateState()
        {
            UpdateState(IAPManager.GetProductData(key));
        }

        public void UpdateState(ProductData product)
        {
            if (product != null)
            {
                loadingObject.SetActive(false);
                priceText.gameObject.SetActive(true);

                backImage.sprite = activeBackSprite;

                priceText.text = product.GetLocalPrice();
            }
            else
            {
                SetDisabledState();
            }
        }

        private void SetDisabledState()
        {
            loadingObject.SetActive(true);
            priceText.gameObject.SetActive(false);

            backImage.sprite = unactiveBackSprite;
        }

        private void OnButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            IAPManager.BuyProduct(key);
        }
    }
}