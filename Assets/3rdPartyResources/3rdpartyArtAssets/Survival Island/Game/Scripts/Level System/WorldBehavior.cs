using Unity.AI.Navigation;
using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    [CustomOverlayElement("Update Prices UI", "OnUpdatePricesButtonClick")]
    public class WorldBehavior : BaseWorldBehavior
    {
        [Space]
        [SerializeField] MissionsHolder missionsHolder;
        public MissionsHolder MissionsHolder => missionsHolder;

        [SerializeField] NavMeshSurface navMeshSurface;

        [Space]
        [SerializeField] SubworldHandler subWorldHandler;
        public SubworldHandler SubworldHandler => subWorldHandler;

        [SerializeField] DiggingSpawnSettings diggingSpawnSettings;
        public DiggingSpawnSettings DiggingSpawnSettings => diggingSpawnSettings;

        [Header("First time loaded")]
        [SerializeField] protected bool resetCurrencyOnFirstTimeLoad;
        [SerializeField, ShowIf("resetCurrencyOnFirstTimeLoad")] protected List<CurrencyPrice> defaultCurrency;

        [SerializeField] bool restoreHungerPointsOnFirstTimeLoad;

        private WorldSave worldSave;

        private void Awake()
        {
            WorldController.SetWorld(this);
        }

        public override void Initialise()
        {
            base.Initialise();

            // Get world save
            worldSave = SaveController.GetSaveObject<WorldSave>(worldSaveName);

            missionsHolder?.Initialise();

            subWorldHandler.Initialise(this);

            // Check if world is loaded for the first time
            if (!worldSave.IsFirstEnter)
            {
                if (resetCurrencyOnFirstTimeLoad)
                {
                    Currency currency = null;

                    // reseting resources
                    for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
                    {
                        currency = CurrenciesController.Currencies[i];

                        if (currency.Amount > 0)
                            CurrenciesController.Set(currency.CurrencyType, 0);
                    }

                    // adding default resources
                    for (int i = 0; i < defaultCurrency.Count; i++)
                    {
                        CurrenciesController.Add(defaultCurrency[i].CurrencyType, defaultCurrency[i].Price);
                    }
                }

                if (restoreHungerPointsOnFirstTimeLoad)
                {
                    if (EnergyController.IsEnergySystemEnabled)
                        EnergyController.RestoreEnergyPoints(float.MaxValue);
                }

                worldSave.IsFirstEnter = true;
            }
        }

        public override void Unload()
        {
            base.Unload();

            taskHandler.Unload();

            subWorldHandler.Unload();
        }

        public void RegisterAndRecalculateNavMesh(SimpleCallback onWorldLoaded)
        {
            NavMeshController.AddNavMeshSurface(navMeshSurface);

            Tween.NextFrame(() => NavMeshController.CalculateNavMesh(onWorldLoaded));
        }

        public static ResourceDropBehavior DropPickableResource(Transform dropParent, Vector3 startPosition, Vector3 hitPosition, int dropAmount, CurrencyType currencyType, DropAnimation dropAnimation = null)
        {
            Currency currency = CurrenciesController.GetCurrency(currencyType);

            GameObject dropObj = currency.Data.DropResPool.GetPooledObject();
            ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();

            dropObj.transform.position = startPosition.AddToY(dropResource.VerticalOffset);
            dropResource.Initialise(dropAmount).SetDropAnimation(dropAnimation).SetDisableTime(30).Throw(dropParent, startPosition, hitPosition);

            return dropResource;
        }

        public void OnSubworldEntered()
        {

        }

        public void OnSubworldLeft()
        {
            EnvironmentController.SetPreset(EnvironmentPresetType);

#if MODULE_CURVE
            if (curveOverride != null)
            {
                curveOverride.Apply();
            }
            else
            {
                if (CurvatureManager.Instance != null)
                {
                    CurvatureManager.Instance.RemoveCurveOverride();
                }
            }
#endif
        }

        public override void OnSceneSaving()
        {
            base.OnSceneSaving();

            if (subWorldHandler.CacheSubworlds())
            {
                RuntimeEditorUtils.SetDirty(this);
            }
        }

#if UNITY_EDITOR
        private void OnUpdatePricesButtonClick()
        {
            List<GroundTileComplexBehavior> groundComplexes = new List<GroundTileComplexBehavior>();
            groundComplexes.AddRange(FindObjectsOfType<GroundTileComplexBehavior>());

            for (int i = 0; i < groundComplexes.Count; i++)
            {
                groundComplexes[i].UpdatePurchaseCostInEditor();
            }

            List<BuildingComplexBehavior> buildingComplexes = new List<BuildingComplexBehavior>();
            buildingComplexes.AddRange(FindObjectsOfType<BuildingComplexBehavior>());

            for (int i = 0; i < buildingComplexes.Count; i++)
            {
                buildingComplexes[i].UpdatePurchaseCostInEditor();
            }
        }
#endif
    }
}