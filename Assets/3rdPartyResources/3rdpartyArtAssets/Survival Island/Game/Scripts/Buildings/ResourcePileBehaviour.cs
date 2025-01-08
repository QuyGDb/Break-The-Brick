using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourcePileBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected CurrencyType currencyToVisualise;
        [SerializeField] Vector3 resourceRotation = Vector3.zero;
        [SerializeField] Vector3 resourceScale = Vector3.one;

        [Header("Pile Dimentions")]
        [SerializeField] Vector3 elementSize = Vector3.one;
        [SerializeField] Vector3 elementsOffset = Vector3.one;

        [Space(5)]
        [SerializeField] int rows = 2;
        [SerializeField] int columns = 2;
        [SerializeField] int layers = 2;

        protected List<Transform> activeObjets = new List<Transform>();

        private bool isActive = true;
        public bool IsActive => isActive;

        private float rowSize;
        private float columnSize;

        public int PileCapacity { get; private set; }

        private Pool currencyObjectsPool;

        private void Awake()
        {
            currencyObjectsPool = CurrenciesController.GetCurrency(currencyToVisualise).Data.FlyingResPool;


            rowSize = elementSize.x * rows + elementsOffset.x * (rows - 1);
            columnSize = elementSize.z * columns + elementsOffset.z * (columns - 1);

            PileCapacity = rows * columns * layers;
        }

        public void AddResources(int amount)
        {
            for (int i = 0; i < amount && activeObjets.Count < PileCapacity; i++)
            {
                // Get object from pool and initialise transform
                GameObject currencyObject = currencyObjectsPool.GetPooledObject();
                currencyObject.transform.ResetLocal();
                currencyObject.transform.position = transform.position + GetElementLocalPosition(activeObjets.Count);
                currencyObject.transform.rotation = Quaternion.Euler(resourceRotation);
                currencyObject.transform.localScale = Vector3.zero;
                currencyObject.SetActive(true);

                // Play scale animation
                currencyObject.transform.DOScale(resourceScale, 0.3f, i * 0.05f).SetEasing(Ease.Type.BackOut);

                // Add element to list
                activeObjets.Add(currencyObject.transform);
            }
        }

        public void RemoveResources(int amount)
        {
            for (int i = 0; i < amount && activeObjets.Count > 0; i++)
            {
                Transform element = activeObjets[activeObjets.Count - 1];
                activeObjets.RemoveAt(activeObjets.Count - 1);

                element.DOScale(0, 0.3f, i * 0.05f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    element.gameObject.SetActive(false);
                });
            }
        }

        private Vector3 GetElementLocalPosition(int index)
        {
            int k = /*layers - 1 -*/ index / (rows * columns); // layer index
            int remainder = index % (rows * columns);
            int i = remainder / columns; // row index
            int j = remainder % columns; // column index


            float x = elementSize.x * i + elementsOffset.x * i + rowSize * -0.5f + elementSize.x * 0.5f;
            float y = elementSize.y * k + elementsOffset.y * k + elementSize.y * 0.5f;
            float z = elementSize.z * j + elementsOffset.z * j + columnSize * -0.5f + elementSize.z * 0.5f;

            return new Vector3(x, y, z);
        }

        private void OnDrawGizmosSelected()
        {
            float rowSize = elementSize.x * rows + elementsOffset.x * (rows - 1);
            float columnSize = elementSize.z * columns + elementsOffset.z * (columns - 1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    for (int k = 0; k < layers; k++)
                    {
                        float x = elementSize.x * i + elementsOffset.x * i + rowSize * -0.5f + elementSize.x * 0.5f;
                        float y = elementSize.y * k + elementsOffset.y * k + elementSize.y * 0.5f;
                        float z = elementSize.z * j + elementsOffset.z * j + columnSize * -0.5f + elementSize.z * 0.5f;

                        Vector3 positionOffset = new Vector3(x, y, z);

                        Gizmos.DrawWireCube(transform.position + positionOffset, elementSize);
                    }
                }
            }
        }

        public void Unload()
        {
            for (int i = 0; i < activeObjets.Count; i++)
            {
                if (activeObjets[i] != null)
                    activeObjets[i].gameObject.SetActive(false);
            }

            activeObjets.Clear();
        }
    }
}