using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceConverterBuildingBehavior : BuildingBehavior
    {
        public int InputStorageCapacity => inputStorageCapacity;
        public int OutputStorageCapacity => outputStorageCapacity;

        [Header("Converter")]
        [SerializeField, HideIf("EditorHaveRecipeUpgrade")] protected Recipe recipe;

        protected ConverterRecipeUpgrade RecipeUpgrade { get; private set; }

        [Space]
        [SerializeField] [HideIf("EditorHaveCapacityUpgrade")] protected int inputStorageCapacity = 10;
        [SerializeField] [HideIf("EditorHaveCapacityUpgrade")] protected int outputStorageCapacity = 10;
        [SerializeField] [HideIf("EditorHaveDurationUpgrade")] protected float conversionDuration = 10;

        [Header("Storage")]
        [SerializeField] ComplexResourceStorageBehavior inStorage;
        public ComplexResourceStorageBehavior InStorage => inStorage;
        [SerializeField] SimpleResourceStorageBehavior outStorage;
        public SimpleResourceStorageBehavior OutStorage => outStorage;

        [Space]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        protected ConverterCapacityUpgrade StorageCapacityUpgrade { get; private set; }
        protected SimpleFloatUpgrade ConversionDurationUpgrade { get; private set; }

        private ConverterStoringTask storingTask;

        private ConverterResourceCanvas inStorageResourceCanvas;
        private ConverterResourceCanvas outStorageResourceCanvas;

        protected override void Init()
        {
            base.Init();

            InStorageInit();
            OutStorageInit();

            inStorageResourceCanvas = (ConverterResourceCanvas)inStorage.ResourceCanvas;
            outStorageResourceCanvas = (ConverterResourceCanvas)outStorage.ResourceCanvas;

            inStorageResourceCanvas.OverrideOriginPositionOfDT(transform.position);
            outStorageResourceCanvas.OverrideOriginPositionOfDT(transform.position);

            UpdateStoragesUISpecialFields();

            if (conversionDuration > 0)
            {
                StartCoroutine(ConvertingCoroutine());
            }

            storingTask = new ConverterStoringTask(this);
            storingTask.Activate();
            storingTask.Register(LinkedWorldBehavior.TaskHandler);
        }

        private void InStorageInit()
        {
            ResourcesList inStorageCapacityList = new ResourcesList();

            for (int i = 0; i < recipe.ComponentsAmount; i++)
            {
                RecipeComponent recipeComponent = recipe.GetComponent(i);

                inStorageCapacityList.Add(new Resource(recipeComponent.ResourceType, recipeComponent.Amount * inputStorageCapacity));
            }

            string storageSaveName = $"{ID}_IngridientsStorage";
            inStorage.Init(storageSaveName, inStorageCapacityList);
        }

        private void OutStorageInit()
        {
            string storageSaveName = $"{ID}_ResultStorage";
            outStorage.Init(storageSaveName, new List<CurrencyType>() { recipe.ResultResourceType }, outputStorageCapacity);
        }

        protected override void RegisterUpgrades()
        {
            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                var upgradeContainer = buildingUpgrades[i];

                switch (upgradeContainer.UpgradeType)
                {
                    case BuildingUpgradeType.StorageCapacity:
                        RegisterCapacityUpgrade(upgradeContainer.Upgrade as ConverterCapacityUpgrade);
                        break;
                    case BuildingUpgradeType.ConversionDuration:
                        RegisterDurationUpgrade(upgradeContainer.Upgrade as SimpleFloatUpgrade);
                        break;
                    case BuildingUpgradeType.Recipe:
                        RegisterRecipeUpgrade(upgradeContainer.Upgrade as ConverterRecipeUpgrade);
                        break;
                }
            }
        }

        protected virtual void RegisterRecipeUpgrade(ConverterRecipeUpgrade upgrade)
        {
            RecipeUpgrade = upgrade;

            var upgradeSaveName = $"{ID}_{BuildingUpgradeType.Recipe}";
            RecipeUpgrade.Init(upgradeSaveName);
            RecipeUpgrade.OnUpgraded += OnRecipeUpgraded;
            recipe = RecipeUpgrade.CurrentStage.Value;
        }

        protected void OnRecipeUpgraded()
        {
            recipe = RecipeUpgrade.CurrentStage.Value;

            inStorage.Clear();
            outStorage.Clear();

            InStorageInit();
            OutStorageInit();

            UpdateStoragesUISpecialFields();
        }

        private void UpdateStoragesUISpecialFields()
        {
            if (inStorageResourceCanvas != null)
            {
                for (int i = 0; i < recipe.ComponentsAmount; i++)
                {
                    RecipeComponent recipeComponent = recipe.GetComponent(i);

                    ConverterResourceUI resUI = inStorageResourceCanvas.ResourceUIList.Find(r => r.CurrencyType.Equals(recipeComponent.ResourceType));
                    resUI.SetFillState(0f);
                    resUI.SetRecipeText("x" + recipeComponent.Amount);
                }
            }

            if (outStorageResourceCanvas != null)
            {
                // as we can have only single type of output product, no need to search it
                ConverterResourceUI resUI = outStorageResourceCanvas.ResourceUIList[0];
                resUI.SetFillState(0f);
                resUI.SetRecipeText("x" + 1);
            }
        }

        protected void OnCapacityUpgraded()
        {
            inputStorageCapacity = StorageCapacityUpgrade.CurrentStage.Value.inputStorageCapacity;
            outputStorageCapacity = StorageCapacityUpgrade.CurrentStage.Value.outputStorageCapacity;

            ResourcesList inStorageCapacityList = new ResourcesList();

            for (int i = 0; i < recipe.ComponentsAmount; i++)
            {
                RecipeComponent recipeComponent = recipe.GetComponent(i);

                inStorageCapacityList.Add(new Resource(recipeComponent.ResourceType, recipeComponent.Amount * inputStorageCapacity));
            }

            InStorage.SetMaxCapacity(inStorageCapacityList);
            outStorage.SetCapacity(outputStorageCapacity);
        }

        protected void OnDurationUpgraded()
        {
            conversionDuration = ConversionDurationUpgrade.CurrentStage.Value;
        }

        private void Update()
        {
            if (conversionDuration <= 0 && !outStorage.IsFull() && inStorage.HasResources(recipe))
            {
                inStorage.RemoveResource(recipe);
                outStorage.AddResources(recipe.ResultResourceType);
                Debug.Log("Add res " + recipe.ResultResourceType);
            }
        }

        private IEnumerator ConvertingCoroutine()
        {
            WaitForSeconds pointTwoSeconds = new WaitForSeconds(0.2f);
            while (true)
            {
                if (outStorage.IsFull() || inStorage.IsEmpty() || !inStorage.HasResources(recipe))
                {
                    outStorageResourceCanvas.ResourceUIList[0].SetFillState(0f);

                    yield return pointTwoSeconds;
                    continue;
                }

                outStorageResourceCanvas.ResourceUIList[0].SetFillState(0f);

                inStorage.RemoveResource(recipe);

                float time = 0f;
                do
                {
                    yield return null;

                    time += Time.deltaTime;

                    float progress = time / conversionDuration;
                    outStorageResourceCanvas.ResourceUIList[0].SetFillState(progress);
                }
                while (time <= conversionDuration);

                outStorage.AddResources(recipe.ResultResourceType);
            }
        }

        protected void RegisterCapacityUpgrade(ConverterCapacityUpgrade upgrade)
        {
            StorageCapacityUpgrade = upgrade;

            string upgradeSaveName = $"{ID}_{BuildingUpgradeType.StorageCapacity}";
            StorageCapacityUpgrade.Init(upgradeSaveName);

            inputStorageCapacity = StorageCapacityUpgrade.CurrentStage.Value.inputStorageCapacity;
            outputStorageCapacity = StorageCapacityUpgrade.CurrentStage.Value.outputStorageCapacity;

            StorageCapacityUpgrade.OnUpgraded += OnCapacityUpgraded;
        }

        protected void RegisterDurationUpgrade(SimpleFloatUpgrade upgrade)
        {
            ConversionDurationUpgrade = upgrade;

            string upgradeSaveName = $"{ID}_{BuildingUpgradeType.ConversionDuration}";
            ConversionDurationUpgrade.Init(upgradeSaveName);

            conversionDuration = ConversionDurationUpgrade.CurrentStage.Value;

            ConversionDurationUpgrade.OnUpgraded += OnDurationUpgraded;
        }
    }
}