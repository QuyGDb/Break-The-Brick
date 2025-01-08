using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(ResourceConverterBuildingBehavior))]
    public class FoodConverterAnimation : MonoBehaviour
    {
        [SerializeField] Transform resultPreviewSpawnPosition;
        [SerializeField] Vector3 resultPreviewRotation;

        private ResourceConverterBuildingBehavior converter;
        private GameObject resultPreview;

        private void Awake()
        {
            converter = GetComponent<ResourceConverterBuildingBehavior>();
        }

        private void OnEnable()
        {
            // tracking changes in the out storage
            converter.OutStorage.OnResourcesChanged += OnOutStorageResourcesChanged;
        }

        private void OnDisable()
        {
            converter.OutStorage.OnResourcesChanged -= OnOutStorageResourcesChanged;
        }

        private void OnOutStorageResourcesChanged()
        {
            // if storage is empty
            if (converter.OutStorage.Storage.IsNullOrEmpty())
            {
                // disable preview
                if (resultPreview != null)
                {
                    resultPreview.SetActive(false);
                    resultPreview = null;
                }
            }
            // if there is a result product
            else
            {
                // spawn preview
                if (resultPreview != null)
                    resultPreview.SetActive(false);

                Currency resultCurrency = CurrenciesController.GetCurrency(converter.OutStorage.Storage[0].currency);
                float verticalOffset = resultCurrency.Data.DropResPrefab.GetComponent<ResourceDropBehavior>().VerticalOffset;

                resultPreview = resultCurrency.Data.FlyingResPool.GetPooledObject(new PooledObjectSettings().SetPosition(resultPreviewSpawnPosition.position.AddToY(verticalOffset)).SetLocalScale(1f).SetRotation(Quaternion.Euler(resultPreviewRotation)));
            }
        }
    }
}