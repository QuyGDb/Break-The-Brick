using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SimpleResourceIndicator : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI amountText;

        public Resource Resource { get; private set; }

        public void SetData(Resource resource)
        {
            Resource = resource;
            iconImage.sprite = CurrenciesController.GetCurrency(resource.currency).Icon;
            amountText.text = resource.amount.ToString();
        }

        public void Assign(Transform parent)
        {
            transform.SetParent(parent);

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void Clear()
        {
            iconImage.sprite = null;
            amountText.text = "";

            gameObject.SetActive(false);
            transform.SetParent(PoolManager.ObjectsContainerTransform);
        }
    }
}
